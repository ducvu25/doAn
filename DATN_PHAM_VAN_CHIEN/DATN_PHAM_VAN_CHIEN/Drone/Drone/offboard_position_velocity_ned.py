import os
import asyncio
from mavsdk import System
import csv
from mavsdk.offboard import PositionNedYaw, VelocityNedYaw, AccelerationNed, OffboardError
from mavsdk.telemetry import LandedState
from mavsdk.action import ActionError
from mavsdk.telemetry import *
import subprocess
import signal
from collections import namedtuple
import functions.global_to_local
import glob
import math

# Constants for APF
K_ATTRACTIVE = 1.0
K_REPULSIVE = 5.0
Q_STAR = 4.0  # Distance threshold for repulsive potential to take effect


def read_hw_id():
    hwid_files = glob.glob('*.hwID')
    if hwid_files:
        filename = hwid_files[0]
        hw_id = os.path.splitext(filename)[0]  # Get filename without extension
        print(f"Hardware ID {hw_id} detected.")
        return int(hw_id)
    else:
        print("Hardware ID file not found.")
        return None

# Constants
STEP_TIME = 0.05
DEFAULT_Z = 0.83
GRPC_PORT_BASE = 50041
SHOW_DEVIATIONS = False
Drone = namedtuple('Drone', 'hw_id pos_id x y ip mavlink_port debug_port gcs_ip')
SIM_MODE = True
SEPERATE_CSV = True 
HW_ID = read_hw_id()


dronesConfig = []


def read_config(filename):
    global dronesConfig
    dronesConfig = []
    with open(filename, newline='') as csvfile:
        reader = csv.reader(csvfile)
        next(reader, None)  # Skip the header
        for row in reader:
            hw_id, pos_id, x, y, ip, mavlink_port, debug_port, gcs_ip = row
            if SIM_MODE == False and int(hw_id) == HW_ID:
                drone = Drone(hw_id, pos_id, float(x), float(y), ip, mavlink_port, debug_port, gcs_ip)
                dronesConfig.append(drone)
                break
            if SIM_MODE:
                drone = Drone(hw_id, pos_id, float(x), float(y), ip, mavlink_port, debug_port, gcs_ip)
                dronesConfig.append(drone)
    return dronesConfig


def read_trajectory_file(filename, trajectory_offset, altitude_offset):
    waypoints = []
    with open(filename, newline="") as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            t = float(row["t"])
            px = float(row["px"]) + trajectory_offset[0]
            py = float(row["py"]) + trajectory_offset[1]
            pz = float(row["pz"]) + trajectory_offset[2] - altitude_offset
            vx = float(row["vx"])
            vy = float(row["vy"])
            vz = float(row["vz"])
            ax = float(row["ax"])
            ay = float(row["ay"])
            az = float(row["az"])
            yaw = float(row["yaw"])
            mode_code = int(row["mode"])
            waypoints.append((t, px, py, pz, vx, vy, vz, ax, ay, az, yaw, mode_code))
    return waypoints


def calculate_attractive_force(current_position, goal_position):
    fx = -K_ATTRACTIVE * (current_position[0] - goal_position[0])
    fy = -K_ATTRACTIVE * (current_position[1] - goal_position[1])
    fz = -K_ATTRACTIVE * (current_position[2] - goal_position[2])
    return fx, fy, fz


def calculate_repulsive_force(current_position, obstacles):
    fx, fy, fz = 0.0, 0.0, 0.0
    for obs in obstacles:
        dist = math.sqrt((current_position[0] - obs[0])**2 + (current_position[1] - obs[1])**2 + (current_position[2] - obs[2])**2)
        if dist < Q_STAR:
            repulsive_strength = K_REPULSIVE * (1.0 / dist - 1.0 / Q_STAR) / (dist**3)
            fx += repulsive_strength * (current_position[0] - obs[0])
            fy += repulsive_strength * (current_position[1] - obs[1])
            fz += repulsive_strength * (current_position[2] - obs[2])
    return fx, fy, fz


def calculate_total_force(current_position, goal_position, obstacles):
    attractive_force = calculate_attractive_force(current_position, goal_position)
    repulsive_force = calculate_repulsive_force(current_position, obstacles)
    total_fx = attractive_force[0] + repulsive_force[0]
    total_fy = attractive_force[1] + repulsive_force[1]
    total_fz = attractive_force[2] + repulsive_force[2]
    return total_fx, total_fy, total_fz


async def perform_trajectory(drone_id, drone, waypoints, home_position, home_position_NED, global_position_telemetry, mode_descriptions):
    print(f"-- Performing trajectory {drone_id}")
    total_duration = waypoints[-1][0]
    t = 0
    last_mode = 0
    last_waypoint_index = 0

      # Positions of other drones as obstacles

    while t <= total_duration:
        actual_position = global_position_telemetry.get(drone_id)
        if actual_position is None:
            await asyncio.sleep(STEP_TIME)
            t += STEP_TIME
            continue

        local_ned_position = functions.global_to_local.global_to_local(actual_position, home_position)
        obstacles = [(d.x, d.y, DEFAULT_Z) for d in dronesConfig if d.hw_id != HW_ID]
        current_waypoint = None
        for i in range(last_waypoint_index, len(waypoints)):
            if t <= waypoints[i][0]:
                current_waypoint = waypoints[i]
                last_waypoint_index = i
                break

        if SEPERATE_CSV:
            position = tuple(a - b for a, b in zip(current_waypoint[1:4], home_position_NED))
        else:
            position = current_waypoint[1:4]

        # Apply APF to calculate the new position
        goal_position = position
        total_force = calculate_total_force((local_ned_position.north_m, local_ned_position.east_m, local_ned_position.down_m), goal_position, obstacles)
        adjusted_position = (
            local_ned_position.north_m + total_force[0] * STEP_TIME,
            local_ned_position.east_m + total_force[1] * STEP_TIME,
            local_ned_position.down_m + total_force[2] * STEP_TIME
        )

        velocity = current_waypoint[4:7]
        acceleration = current_waypoint[7:10]
        yaw = current_waypoint[10]
        mode_code = current_waypoint[-1]
        if last_mode != mode_code:
            print(f"Drone id: {drone_id + 1}: Mode number: {mode_code}, Description: {mode_descriptions[mode_code]}")
            last_mode = mode_code

        await drone.offboard.set_position_velocity_acceleration_ned(
            PositionNedYaw(*adjusted_position, yaw),
            VelocityNedYaw(*velocity, yaw),
            AccelerationNed(*acceleration)
        )

        await asyncio.sleep(STEP_TIME)
        t += STEP_TIME

        if int(t / STEP_TIME) % 100 == 0:
            deviation = [(a - b) for a, b in zip(position, [local_ned_position.north_m, local_ned_position.east_m, local_ned_position.down_m])]
            if SHOW_DEVIATIONS:
                print(f"Drone {drone_id + 1} Deviations: {round(deviation[0], 1)} {round(deviation[1], 1)} {round(deviation[2], 1)}")

    print(f"-- Shape completed {drone_id + 1}")


async def get_global_position_telemetry(drone_id, drone):
    async for global_position in drone.telemetry.position():
        global_position_telemetry[drone_id] = global_position


async def initial_setup_and_connection(drone_id, udp_port):
    dronesConfig = read_config('config.csv')
    if SIM_MODE:
        grpc_port = GRPC_PORT_BASE + drone_id
    else:
        grpc_port = GRPC_PORT_BASE - 1
    home_position = None

    mode_descriptions = {
        0: "On the ground",
        10: "Initial climbing state",
        20: "Initial holding after climb",
        30: "Moving to start point",
        40: "Holding at start point",
        50: "Moving to maneuvering start point",
        60: "Holding at maneuver start point",
        70: "Maneuvering (trajectory)",
        80: "Holding at the end of the trajectory coordinate",
        90: "Returning to home coordinate",
        100: "Landing"
    }

    drone = System(mavsdk_server_address="127.0.0.1", port=grpc_port)
    await drone.connect(system_address=f"udp://:{udp_port}")
    print(f"Drone connecting with UDP: {udp_port}")

    # Ensure future telem data
    asyncio.ensure_future(get_global_position_telemetry(drone_id, drone))

    # Check if the drone is connected
    async for state in drone.core.connection_state():
        if state.is_connected:
            print(f"Drone id {drone_id + 1} connected on Port: {udp_port} and grpc Port: {grpc_port}")
            break

    return drone, mode_descriptions, home_position


async def pre_flight_checks(drone_id, drone):
    # Wait for the drone to have a global position estimate
    async for health in drone.telemetry.health():
        if health.is_global_position_ok and health.is_home_position_ok:
            print(f"Global position estimate ok {drone_id + 1}")
            home_position = global_position_telemetry.get(drone_id)
            if home_position is not None:
                print(f"Home Position of {drone_id + 1} set to: {home_position}")
                break

    return home_position


async def arming_and_starting_offboard_mode(drone_id, drone):
    print(f"-- Arming {drone_id + 1}")
    await drone.action.arm()
    print(f"-- Setting initial setpoint {drone_id + 1}")
    await drone.offboard.set_position_ned(PositionNedYaw(0.0, 0.0, 0.0, 0.0))
    print(f"-- Starting offboard {drone_id + 1}")
    try:
        await drone.offboard.start()
    except OffboardError as error:
        print(f"-- Disarming {drone_id + 1}")
        await drone.action.disarm()
        return


async def perform_landing(drone_id, drone):
    print(f"-- Landing {drone_id + 1}")
    await drone.action.land()

    async for state in drone.telemetry.landed_state():
        if state == LandedState.ON_GROUND:
            break


async def stop_offboard_mode(drone_id, drone):
    print(f"-- Stopping offboard {drone_id + 1}")
    try:
        await drone.offboard.stop()
    except Exception as error:
        print(f"Stopping offboard mode failed with error: {error}")


async def disarm_drone(drone_id, drone):
    print(f"-- Disarming {drone_id + 1}")
    await drone.action.disarm()


async def create_drone_configurations(num_drones, time_offset):
    # Define altitude offsets for each drone
    altitude_steps = 0
    altitude_offsets = [altitude_steps * i for i in range(num_drones)]

    # relative to drone 0
    home_positions = [(drone.x, drone.y, DEFAULT_Z) for drone in dronesConfig]
    traejctory_offset = [(0, 0, 0) for i in range(num_drones)]
    if SIM_MODE == False:
        udp_ports = []
        udp_ports.append(14540)  # default api connection on same hardware
    else:
        udp_ports = [drone.mavlink_port for drone in dronesConfig]
    return home_positions, traejctory_offset, udp_ports, altitude_offsets


def start_mavsdk_servers(num_drones, udp_ports):
    # Start mavsdk_server instances for each drone
    mavsdk_servers = []
    for i in range(num_drones):
        if SIM_MODE == True:
            port = GRPC_PORT_BASE + i
        else:
            port = 50040
        mavsdk_server = subprocess.Popen(["./mavsdk_server", "-p", str(port), f"udp://:{udp_ports[i]}"])
        mavsdk_servers.append(mavsdk_server)
    return mavsdk_servers


async def run_all_drones(num_drones, home_positions, traejctory_offset, udp_ports, time_offset, altitude_offsets):
    tasks = []
    for i in range(num_drones):
        if SIM_MODE == True:
            drone_id = i
        else:
            drone_id = HW_ID
        tasks.append(asyncio.create_task(run_drone(drone_id, home_positions[i], traejctory_offset[i], udp_ports[i], i * time_offset, altitude_offsets[i])))
    await asyncio.gather(*tasks)


def stop_all_mavsdk_servers(mavsdk_servers):
    # Kill all mavsdk_server processes
    for mavsdk_server in mavsdk_servers:
        os.kill(mavsdk_server.pid, signal.SIGTERM)


async def run_drone(drone_id, home_position_NED, trajectory_offset, udp_port, time_offset, altitude_offset):
    if SIM_MODE == False:
        drone_id = 0
    # Call the initial setup and connection function
    drone, mode_descriptions, home_position = await initial_setup_and_connection(drone_id, udp_port)

    # Add time offset before starting the maneuver
    await asyncio.sleep(time_offset)

    # Perform pre-flight checks
    home_position = await pre_flight_checks(drone_id, drone)

    # Arm the drone and start offboard mode
    await arming_and_starting_offboard_mode(drone_id, drone)

    if SEPERATE_CSV:
        if SIM_MODE == False:
            filename = "shapes/swarm/processed/Drone " + str(HW_ID) + ".csv"
        else:
            filename = "shapes/swarm/processed/Drone " + str(drone_id + 1) + ".csv"
    else:
        filename = "shapes/active.csv"

    waypoints = read_trajectory_file(filename, trajectory_offset, altitude_offset)

    await perform_trajectory(drone_id, drone, waypoints, home_position, home_position_NED, global_position_telemetry, mode_descriptions)

    # Perform landing
    await perform_landing(drone_id, drone)

    # Stop offboard mode
    await stop_offboard_mode(drone_id, drone)

    # Disarm the drone
    await disarm_drone(drone_id, drone)


async def main():
    num_drones = len(read_config('config.csv'))
    time_offset = 0

    home_positions, traejctory_offset, udp_ports, altitude_offsets = await create_drone_configurations(num_drones, time_offset)
    mavsdk_servers = start_mavsdk_servers(num_drones, udp_ports)
    await run_all_drones(num_drones, home_positions, traejctory_offset, udp_ports, time_offset, altitude_offsets)
    stop_all_mavsdk_servers(mavsdk_servers)
    print("All tasks completed. Exiting program.")


if __name__ == "__main__":
    global_position_telemetry = {}
    asyncio.run(main())

import asyncio
import csv
import os
import subprocess
import signal
import time
from mavsdk import System
from mavsdk.offboard import PositionNedYaw, VelocityNedYaw, AccelerationNed, OffboardError
from mavsdk.telemetry import LandedState


async def save_trajectory_to_csv(file_name, trajectory_data):
    """
    Saves the trajectory data of UAV to a CSV file.
    :param file_name: The name of the CSV file to save data
    :param trajectory_data: List of trajectory data
    """
    try:
        with open(file_name, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(["Time", "North", "East", "Down", "Velocity_North", "Velocity_East", 
                             "Velocity_Down", "Acceleration_North", "Acceleration_East", 
                             "Acceleration_Down", "Yaw", "Mode"])
            for data in trajectory_data:
                writer.writerow(data)
        print(f"Trajectory saved to {file_name}")
    except Exception as e:
        print(f"Error saving trajectory to CSV: {e}")


async def read_waypoints_from_csv(file_path):
    """
    Reads waypoints from a CSV file.
    :param file_path: Path to the CSV file
    :return: List of waypoints
    """
    waypoints = []
    try:
        with open(file_path, newline="") as csvfile:
            reader = csv.DictReader(csvfile)
            for row in reader:
                t = float(row["t"])
                px = float(row["px"])
                py = float(row["py"])
                pz = float(row["pz"])
                vx = float(row["vx"])
                vy = float(row["vy"])
                vz = float(row["vz"])
                ax = float(row["ax"])
                ay = float(row["ay"])
                az = float(row["az"])
                yaw = float(row["yaw"])
                mode_code = int(row["mode"])

                waypoints.append((t, px, py, pz, vx, vy, vz, ax, ay, az, yaw, mode_code))
    except Exception as e:
        print(f"Error reading waypoints from CSV: {e}")
    
    return waypoints


async def connect_to_drone(grpc_port):
    """
    Connects to the drone.
    :param grpc_port: Port to connect to the mavsdk_server
    :return: The drone system object
    """
    drone = System(mavsdk_server_address="127.0.0.1", port=grpc_port)
    await drone.connect(system_address="udp://:14540")

    print("Waiting for drone to connect...")
    async for state in drone.core.connection_state():
        if state.is_connected:
            print("-- Connected to drone!")
            break

    print("Waiting for drone to have a global position estimate...")
    async for health in drone.telemetry.health():
        if health.is_global_position_ok and health.is_home_position_ok:
            print("-- Global position estimate OK")
            break

    return drone


async def start_offboard_mode(drone):
    """
    Starts the offboard mode.
    :param drone: The drone system object
    :return: None
    """
    print("-- Arming")
    await drone.action.arm()

    print("-- Setting initial setpoint")
    await drone.offboard.set_position_ned(PositionNedYaw(0.0, 0.0, 0.0, 0.0))

    print("-- Starting offboard mode")
    try:
        await drone.offboard.start()
    except OffboardError as error:
        print(f"Starting offboard mode failed with error code: {error._result.result}")
        print("-- Disarming")
        await drone.action.disarm()


async def execute_trajectory(drone, waypoints, mode_descriptions):
    """
    Executes the UAV trajectory based on waypoints.
    :param drone: The drone system object
    :param waypoints: List of waypoints for the trajectory
    :param mode_descriptions: Dictionary of mode codes and their descriptions
    :return: List of trajectory data
    """
    total_duration = waypoints[-1][0]  # Total duration is the time of the last waypoint
    trajectory_data = []
    last_mode = 0
    start_time = time.time()

    while time.time() - start_time <= total_duration:
        elapsed_time = time.time() - start_time
        current_waypoint = next((wp for wp in waypoints if wp[0] >= elapsed_time), None)

        if current_waypoint is None:
            break

        position = current_waypoint[1:4]
        velocity = current_waypoint[4:7]
        acceleration = current_waypoint[7:10]
        yaw = current_waypoint[10]
        mode_code = current_waypoint[-1]

        # Print mode description if it changes
        if last_mode != mode_code:
            print(f"Mode number: {mode_code}, Description: {mode_descriptions[mode_code]}")
            last_mode = mode_code

        # Send command to UAV
        await drone.offboard.set_position_velocity_acceleration_ned(
            PositionNedYaw(*position, yaw),
            VelocityNedYaw(*velocity, yaw),
            AccelerationNed(*acceleration)
        )

        # Add this waypoint data to the trajectory list
        trajectory_data.append((
            elapsed_time, *position, *velocity, *acceleration, yaw, mode_code
        ))

        await asyncio.sleep(0.1)

    return trajectory_data


async def main():
    udp_port = 14540
    grpc_port = 50040

    # Start mavsdk_server
    mavsdk_server = subprocess.Popen(["./mavsdk_server", "-p", str(grpc_port), f"udp://:{udp_port}"])

    # Connect to drone and setup
    drone = await connect_to_drone(grpc_port)
    mode_descriptions = {
        0: "On the ground", 10: "Initial climbing state", 20: "Initial holding after climb",
        30: "Moving to start point", 40: "Holding at start point", 50: "Moving to maneuvering start point",
        60: "Holding at maneuver start point", 70: "Maneuvering (trajectory)", 
        80: "Holding at the end of the trajectory coordinate", 90: "Returning to home coordinate", 
        100: "Landing"
    }

    # Start offboard mode
    await start_offboard_mode(drone)

    # Read waypoints from CSV
    waypoints = await read_waypoints_from_csv("shapes/active.csv")

    # Execute trajectory
    trajectory_data = await execute_trajectory(drone, waypoints, mode_descriptions)

    # Save trajectory data to CSV
    await save_trajectory_to_csv("drone_trajectory.csv", trajectory_data)

    print("-- Shape completed")
    print("-- Landing")
    await drone.action.land()

    async for state in drone.telemetry.landed_state():
        if state == LandedState.ON_GROUND:
            break

    print("-- Stopping offboard mode")
    try:
        await drone.offboard.stop()
    except Exception as error:
        print(f"Stopping offboard mode failed with error: {error}")

    print("-- Disarming")
    await drone.action.disarm()

    # Kill mavsdk_server
    os.kill(mavsdk_server.pid, signal.SIGTERM)
    print("All tasks completed. Exiting program.")


if __name__ == "__main__":
    asyncio.run(main())

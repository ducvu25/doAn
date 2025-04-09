import sys
import math
import csv
import time
import numpy as np
from PyQt5.QtWidgets import QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, QPushButton, QLabel, QFrame, QGroupBox, QFormLayout, QScrollArea
from PyQt5.Qt3DExtras import Qt3DWindow, QOrbitCameraController, QSphereMesh, QPhongMaterial
from PyQt5.Qt3DCore import QEntity, QTransform
from PyQt5.QtGui import QVector3D, QColor
from PyQt5.Qt3DExtras import QCylinderMesh
from PyQt5.QtCore import QTimer
import subprocess

class DroneLightShowApp(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Mô Phỏng Trình Diễn Ánh Sáng UAV")
        self.setGeometry(100, 100, 1200, 800)  # Giao diện chính gọn hơn để phù hợp với màn hình máy tính

        # Thiết lập giao diện chính
        mainWidget = QWidget()
        mainLayout = QHBoxLayout(mainWidget)

        # Thiết lập giao diện 3D
        self.view3D = Qt3DWindow()
        self.container = self.createWindowContainer(self.view3D)
        self.container.setMinimumSize(900, 800)

        # Thực thể gốc cho cảnh 3D
        self.rootEntity = QEntity()
        self.view3D.setRootEntity(self.rootEntity)
        self.view3D.setRootEntity(self.rootEntity)

        # Thiết lập camera
        self.camera = self.view3D.camera()
        self.camera.lens().setPerspectiveProjection(45.0, 16.0 / 9.0, 0.1, 1000.0)
        self.camera.setPosition(QVector3D(0, -100, 50))  # Thay đổi góc nhìn từ phía dưới mặt đất nhìn lên
        self.camera.setViewCenter(QVector3D(0, -100, 0))

        # Bộ điều khiển camera
        self.camController = QOrbitCameraController(self.rootEntity)
        self.camController.setLinearSpeed(50.0)
        self.camController.setLookSpeed(360.0)
        self.camController.setCamera(self.camera)

        # Bố cục bảng điều khiển (thanh bên trái)
        controlWidget = QFrame()
        controlWidget.setFrameShape(QFrame.StyledPanel)
        controlLayout = QVBoxLayout(controlWidget)

        # Thêm các điều khiển
        self.startButton = QPushButton("Bắt Đầu Trình Diễn Ánh Sáng")
        self.startButton.clicked.connect(self.start_lightshow)
        controlLayout.addWidget(self.startButton)

        self.stopButton = QPushButton("Dừng Trình Diễn Ánh Sáng")
        self.stopButton.clicked.connect(self.stop_lightshow)
        controlLayout.addWidget(self.stopButton)

        self.infoLabel = QLabel("Mô Phỏng Trình Diễn Ánh Sáng UAV")
        controlLayout.addWidget(self.infoLabel)

        # Khu vực chứa thông tin UAV với thanh cuộn
        scrollArea = QScrollArea()
        scrollArea.setWidgetResizable(True)
        scrollContent = QWidget()
        scrollLayout = QVBoxLayout(scrollContent)

        # Thêm các khung thông tin drone
        self.droneLabels = []
        for i in range(1, 10):  # Giả định 30 drone
            droneGroupBox = QGroupBox(f"Thông tin Drone {i}")
            formLayout = QFormLayout()            
            idLabel = QLabel(f"ID: {i}")
            positionLabel = QLabel("Vị trí: (0.00, 0.00, 0.00)")
            modeLabel = QLabel("Mode: Offboard")
            ledColorLabel = QLabel("Màu LED: Trắng")
            statusLabel = QLabel("Trạng thái: Chưa Hoạt Động")
            
            formLayout.addRow( idLabel)
            formLayout.addRow( positionLabel)
            formLayout.addRow(modeLabel)
            formLayout.addRow( ledColorLabel)
            formLayout.addRow( statusLabel)
            droneGroupBox.setLayout(formLayout)

            scrollLayout.addWidget(droneGroupBox)
            self.droneLabels.append((positionLabel, ledColorLabel, statusLabel))

        scrollArea.setWidget(scrollContent)
        controlLayout.addWidget(scrollArea)

        # Thêm bộ chứa 3D và bảng điều khiển vào bố cục chính với tỉ lệ 1:3
        mainLayout.addWidget(controlWidget, 1)
        mainLayout.addWidget(self.container, 3)

        self.setCentralWidget(mainWidget)

        # Khởi tạo drone
        self.drones = []
        self.trajectories = []
        self.init_drones()

        # Tạo Timer để cập nhật vị trí drone
        self.timer = QTimer()
        self.timer.timeout.connect(self.update_drones_positions)
        self.running = False
        self.current_step = 0

    def read_drones_config(self, filename):
        drones_positions = []
        try:
            with open(filename, newline='') as csvfile:
                reader = csv.DictReader(csvfile)
                for row in reader:
                    x = float(row['x'])
                    y = float(row['y'])
                    z = 0.0  # Giả định cao độ ban đầu là 0 cho tất cả drone
                    drones_positions.append([x, y, z])
            print(f"Đã tải vị trí drone từ {filename}")
        except FileNotFoundError:
            print(f"Lỗi: Không tìm thấy tệp {filename}.")
        return drones_positions

    def read_trajectory_file(self, filename):
        waypoints = []
        try:
            with open(filename, newline='') as csvfile:
                reader = csv.DictReader(csvfile)
                for row in reader:
                    px = float(row['px'])
                    py = float(row['py'])
                    pz = float(row['pz'])
                    waypoints.append([px, py, pz])
            print(f"Đã tải quãng đường từ {filename}")
        except FileNotFoundError:
            print(f"Lỗi: Không tìm thấy tệp {filename}.")
        return waypoints

    def init_drones(self, num_drones=10):
        # Tạo thực thể drone dựa trên cấu hình
        drones_positions = self.read_drones_config('config.csv')
        for i, pos in enumerate(drones_positions):
            drone = QEntity(self.rootEntity)

            # Tạo hình ảnh đại diện cho drone
            mesh = QSphereMesh()
            mesh.setRadius(0.5)  # Kích thước drone lớn hơn
            material = QPhongMaterial()
            material.setDiffuse(QColor(255, 255, 255))  # Màu LED mặc định là trắng

            # Biến đổi drone
            transform = QTransform()
            transform.setTranslation(QVector3D(pos[0], pos[1], -pos[2]))

            # Thiết lập thực thể drone
            drone.addComponent(mesh)
            drone.addComponent(material)
            drone.addComponent(transform)

            # Lưu drone vào danh sách cùng biến đổi để di chuyển sau này
            self.drones.append((drone, transform))

    def start_lightshow(self):
        try:
            subprocess.Popen(["python3", "offboard_position_velocity_ned.py"])
            print("Đã khởi chạy file offboard_position_velocity_ned.py")
        except FileNotFoundError:
            print("Lỗi: Không tìm thấy file offboard_position_velocity_ned.py")
        # Tải các quãng đường cho mỗi drone từ tệp CSV riêng
        self.trajectories = []
        for i in range(len(self.drones)):
            filename = f'shapes/swarm/processed/Drone {i+1}.csv'
            waypoints = self.read_trajectory_file(filename)
            self.trajectories.append(waypoints)

        # Bắt đầu trình diễn ánh sáng
        if len(self.trajectories) > 0:
            self.infoLabel.setText("Đã bắt đầu trình diễn: Các drone đang theo dõi quãng đường!")
            self.running = True
            self.current_step = 0
            self.timer.start(100)  # Cập nhật mỗi 100ms
        else:
            self.infoLabel.setText("Trình diễn không thể bắt đầu: Không tải được quãng đường!")

    def stop_lightshow(self):
        # Dừng trình diễn ánh sáng
        if self.running:
            self.timer.stop()
            self.running = False
            self.infoLabel.setText("Trình diễn đã dừng!")

    def update_drones_positions(self):
        # Cập nhật vị trí drone theo thời gian
        if not self.running:
            return

        max_steps = max(len(trajectory) for trajectory in self.trajectories)
        if self.current_step < max_steps:
            for i, (_, transform) in enumerate(self.drones):
                if self.current_step < len(self.trajectories[i]):
                    waypoint = self.trajectories[i][self.current_step]
                    transform.setTranslation(QVector3D(waypoint[0], waypoint[1], -waypoint[2]))
                    # Cập nhật trạng thái nhãn drone
                    position_label, led_color_label, status_label = self.droneLabels[i]
                    position_label.setText(f"Vị trí: ({waypoint[0]:.2f}, {waypoint[1]:.2f}, {-waypoint[2]:.2f})")
                    status_label.setText("Trạng thái: Đang Hoạt Động")
            # Cập nhật trung tâm nhìn của camera đến gốc tọa độ (0, -50, 0)
            self.camera.setViewCenter(QVector3D(0, -50, 0))
            QApplication.processEvents()
            self.current_step += 1
        else:
            self.stop_lightshow()


if __name__ == "__main__":
    app = QApplication(sys.argv)
    mainWin = DroneLightShowApp()
    mainWin.show()
    sys.exit(app.exec_())
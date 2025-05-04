import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

# Load the CSV file
csv_path = 'C:\\Learn\\doAn\\DoAnDrone\\Assets\\Resources\\drone_profiler.csv'
df = pd.read_csv(csv_path)

# Extract time and drone position columns
time = df['time'].values
pos_cols = [col for col in df.columns if col.startswith('d')]
num_drones = len(pos_cols) // 3

# Reshape positions into (samples, num_drones, 3)
positions = df[pos_cols].values.reshape(-1, num_drones, 3)

# Calculate total distance traveled for each drone
total_distances = np.zeros(num_drones)
for i in range(1, positions.shape[0]):
    deltas = positions[i] - positions[i - 1]
    step_distances = np.linalg.norm(deltas, axis=1)
    total_distances += step_distances

# Create a DataFrame to show the result
total_distance_df = pd.DataFrame({
    'Drone Index': np.arange(num_drones),
    'Total Distance Traveled': total_distances
})

# Tổng quãng đường của tất cả drone
total_distance_all_drones = total_distances.sum()
print(f"Tổng quãng đường di chuyển của tất cả drone: {total_distance_all_drones:.2f}")

# Plot total distance traveled by each drone
plt.figure(figsize=(8, 5))
plt.bar(total_distance_df['Drone Index'], total_distance_df['Total Distance Traveled'])
plt.xlabel('Drone Index')
plt.ylabel('Total Distance Traveled')
plt.title('Total Distance Traveled by Each Drone')
plt.grid(True, axis='y')
plt.figtext(0.5, -0.05, f"Tổng quãng đường của tất cả drone: {total_distance_all_drones:.2f} đơn vị", 
            wrap=True, horizontalalignment='center', fontsize=10)
plt.tight_layout()
plt.show()

import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

# Replace with the actual path if needed
csv_path = 'C:\\Learn\\doAn\\DoAnDrone\\Assets\\Resources\\drone_profiler1.csv'

# Read the CSV file
df = pd.read_csv(csv_path)

# Extract time column
time = df['time'].values

# Identify all drone position columns (assumes pattern d{index}_x, d{index}_y, d{index}_z)
pos_cols = [col for col in df.columns if col.startswith('d')]

# Number of drones
num_drones = len(pos_cols) // 3

# Reshape data into (samples, num_drones, 3)
positions = df[pos_cols].values.reshape(-1, num_drones, 3)

# Compute average pairwise distance for each time sample
min_distances = []
for sample in positions:
    dists = np.sqrt(((sample[:, None, :] - sample[None, :, :]) ** 2).sum(axis=2))
    i_upper = np.triu_indices(num_drones, k=1)
    min_distances.append(dists[i_upper].min())

min_distances = np.array(min_distances)

# Plot average distance over time
plt.figure()
plt.plot(time, min_distances)
plt.xlabel('Time (s)')
plt.ylabel('Min Pairwise Distance')
plt.title('Min Distance Between Drones Over Time')
plt.tight_layout()
plt.show()

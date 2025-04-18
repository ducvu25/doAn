![MAVSDK Basid Drone Show (1)](https://github.com/alireza787b/mavsdk_drone_show/assets/30341941/acc6aec0-2e24-4822-86d8-9928223c8080)





<!DOCTYPE html>
<html>

<body>
   <h1>Drone Show Basics with Custom Shapes using MAVSDK Offboard Control</h1>
   <p>Welcome to this repository, which provides a tutorial on creating captivating drone shows with custom shapes using MAVSDK's offboard control. This tutorial covers the basics of offboard mode in PX4 and demonstrates its capabilities with a single drone. Please note that the current implementation in MAVSDK does not support feedforwarding acceleration while setting position and velocity in offboard mode. However, these tutorials serve as a starting point for understanding offboard control and its potential for coordinating drone shows.</p>

   <h2>Version 0.3: Enhanced Drone Show Control and Optimizations</h2>
  <a href="https://youtu.be/wctmCIzpMpY" target="_blank"><img src="https://github.com/alireza787b/mavsdk_drone_show/assets/30341941/668b1713-62f1-4c06-886f-8d4ae870e115" style="width="300px" /></a>
   <p>With the new v0.3 update, we have made significant improvements and added several new features:</p>
   <ul>
     <li><strong>Skybrush CSV Processing:</strong> We've added the ability to process Skybrush CSV files and convert them into our template. This feature also includes creating acceleration and velocities for improved path following. A detailed video tutorial for this is available on our YouTube channel .</li>
      <li><a href="https://youtu.be/wctmCIzpMpY">YouTube Tutorial on "Using Sky Brush and Blender for PX4 MAVSDK Drone Shows: Improving our Drone Show Project"</a></li>
     <li><strong>Coordinator.py:</strong> This new script manages and times the mission and state telemetry, enhancing the control of the drone show.</li>
     <li><strong>Merged Control Files:</strong> We have merged the multiple and single, as well as real and simulation mode control files for a more streamlined user experience.</li>
     <li><strong>Code Optimization:</strong> The code has been optimized for better performance and readability.</li>
     <li><strong>Bug Fixes:</strong> Various bugs have been identified and resolved in this update.</li>
   </ul>
   
   <p>The previous versions, v0.1 and v0.2, are still functional and provide a basis for single drone shows and multiple drone shows respectively. We recommend starting with these versions if you're new to drone programming.</p>

   <p>As always, exercise caution and ensure you have the necessary expertise before using offboard mode in a real-world setting. Happy drone programming!</p>

  <h2>Introduction:</h2>
  <p>MAVSDK (MAVLink SDK) is a powerful tool for interacting with drones using the MAVLink communication protocol. This tutorial focuses on the offboard mode in PX4, which enables external systems to directly control a drone's position and velocity. Offboard mode allows for autonomous flight and finds applications in research, development, and testing scenarios.</p>
<a href="https://mavsdk.mavlink.io/main/en/">MAVSDK Documentation</a>

  <h2>Prerequisites:</h2>
  <p>To run the code and follow along with the tutorial, ensure that you have the following prerequisites in place:</p>
  <ul>
    <li>Python: Install Python and the required dependencies for running the scripts.</li>
    <li>PX4 Development Environment for SITL: Set up a PX4 Software-in-the-Loop simulation environment on either Ubuntu or WSL-2. For detailed instructions, refer to the tutorial available on our YouTube channel.</li>
    <li>MAVSDK: Install MAVSDK, which provides the necessary libraries for communication with the drone. Detailed installation and setup instructions can be found in our MAVSDK tutorial video on YouTube.</li>
  </ul>
  <p>Note: When using offboard mode with a real drone, it is essential to have a companion computer (such as a Raspberry Pi) onboard the drone or a reliable telemetry system. Please exercise caution and ensure you have the necessary expertise and understanding of offboard mode before attempting to use it with a real drone. Consider safety measures and be prepared for failsafe scenarios.</p>
  <h2>Tutorial Highlights:</h2>
  <p>In this tutorial, we will guide you through a simulated environment using SITL PX4 in Gazebo. The demonstration includes a single drone performing captivating shapes such as circles, squares, hearts, and even a helix. While the current implementation in MAVSDK may not provide the same level of performance as using MAVROS with feedforwarded accelerations, this tutorial serves as an educational starting point for understanding offboard control.</p>
  <p>Make sure to check out the complete code, resources, and sample CSV files in our GitHub repository. Additionally, we invite you to subscribe to our YouTube channel for more exciting tutorials and drone-related content.</p>
  <h3>Useful Tutorials:</h3>
  <ul>
    <li><a href="https://www.youtube.com/watch?v=iVU8ZNoMn_U">YouTube Tutorial on how to build your WSL-2 PX4 Development Environment</a></li>
    <li><a href="https://www.youtube.com/watch?v=SM0WtREzqqE">YouTube Tutorial on how to install and start with MAVSDK</a></li>
    <li><a href="https://www.youtube.com/watch?v=p0WAPgaa7Rs&list=PLVZvZdBQdm_67sRE_2xUMxYhM41z00Ciz">My PX4 Development YouTube Playlist</a></li>
    <li><a href="https://youtu.be/Wtxsv9mLkEU">Multiple Drone Show Youtube Demo</a></li>

  </ul>
  <h2>Usage:</h2>
  <p>Follow the YouTube Tutorial for this project:</p>
  <ul>
        <li><a href="https://www.youtube.com/watch?v=dg5jyhV15S8">Youtube Demo for this Repository</a></li>
  </ul>

  <p>You can use csvCreator.py to create different shape setpoints. active.csv and a screenshot of the drone's 3D trajectory will be saved to the shapes folder. The next step is to run offboard_from_csv.py and see the drone following the actions.</p>
  <h2>Disclaimer:</h2>
  <p><strong>Using offboard mode with a real drone involves risks and should be approached with caution.</strong> Understand the safety implications, consider failsafe scenarios, and ensure you have the necessary expertise before using offboard mode in a real-world setting.</p>
</body>
</html>

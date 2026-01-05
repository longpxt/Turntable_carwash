![infinity](https://github.com/user-attachments/assets/99d198d9-0916-4e60-9176-2fb3f5b57d2e)
![model](https://github.com/user-attachments/assets/953bfc2e-ba61-43e1-a0f5-f05d72d34021)


# Turntable Car Wash (aka “Tron Car Wash”)

An open-hardware, industrial-scale in-bay car wash architecture that replaces the traditional moving overhead gantry with a **servo-driven turntable** and **parallel Spray Column modules**.

> Goal: increase throughput, simplify hose/cable routing, and improve maintainability by “inverting” the conventional in-bay design.

---

## Context and motivation

Most in-bay automatic car washes rely on an overhead gantry that moves back-and-forth along the vehicle. While widely adopted, this approach tends to increase mechanical complexity and adds stress to **hose routing** and **cable management** due to constant motion and repeated travel paths.

This project explores a different architecture aimed at:
- reducing moving-axis complexity,
- enabling easier maintenance and modular expansion,
- improving process parallelism (multiple wash actions at once).

---

## The inverted architecture

**Turntable Car Wash** reverses the common approach:

- The **vehicle rotates** on a servo-driven turntable (angle-accurate rotation).
- The washing hardware is organized as **stationary modules** (Spray Columns) around the vehicle.
- By eliminating the moving gantry, **hose/cable runs can be shorter**, less twisted, and easier to service.

---

## Parallel washing modules (Spray Columns)

With the gantry eliminated, multiple **Spray Column** modules can work in parallel:
- Pre-soak, wash, rinse, wax, and spot-free can be distributed across modules.
- The system can reduce reliance on repeated serial gantry passes.
- Control logic focuses on *orchestrating parallel actions* and optimizing rotation timing.

---

## System structure

Key subsystems include:
- **Servo-driven turntable** for smooth, angle-accurate rotation
- **Spray Column modules** (modular washing columns)
- **PLC control** for deterministic motion + sequencing
- **Embedded PC software** 
- **Electrical system** integrating PLC, servo drive(s), pumps and sensors.
- **Industrial mechanical structure** designed for replication and serviceability (sheet metal/bolted assemblies)

---

## Repository layout

├─ Spray column/
│ └─ SolidWorks design files for the washing module (Spray Column)
├─ Turntable/
│ └─ SolidWorks design files for the servo-driven turntable
├─ Software & PLC/
│ ├─ PLC source code (control logic, sequencing, motion)
│ └─ Embedded PC software source code 
└─ Electrical/
└─ Electrical drawings for PLC, servo, pumps, and overall control system


---

## Prototype status and early observations

An MVP prototype with a single wash module has been built and tested. Early observations indicate that:
- The architecture is feasible for in-bay operation.
- Optimization of rotation + spray timing has a measurable impact on wash quality and resource usage.
- Long-term reliability will depend heavily on the turntable subsystem design (sealing, drainage, and protection in wet/chemical environments).

---

## Known engineering challenges / trade-offs

The **turntable** is the most demanding subsystem, with challenges including:
- long-life bearings and sealing in a wet, chemical environment,
- debris / grit intrusion protection,
- drainage and corrosion mitigation,
- mechanical rigidity under vehicle load,
- safety interlocks and fault handling for rotating machinery.

---

## Demonstration

Video demo: https://youtu.be/eL5WlzASyZ8

---

## Open release

This project is released as **open hardware** under the **CERN Open Hardware Licence (CERN-OHL-W)**.

- Hardware files: CAD (SolidWorks), electrical drawings
- Control: PLC code
- Software: embedded PC components

See the `LICENSE` file for the exact terms.

---

## Safety notice

This is an **industrial machine concept** involving electricity, rotating machinery, pressurized water, and chemicals.
You are responsible for ensuring compliance with local electrical/mechanical codes, safety guarding, E-stop strategy, interlocks, and chemical handling requirements before building or operating any derivative system.

---

## Contributing

Contributions are welcome:
- mechanical design improvements (turntable sealing/drainage, modularization)
- PLC sequencing and safety logic
- embedded software
- documentation (assembly, commissioning, maintenance)

---

## Author

**Long Phan** — inventor and R&D contractor working across mechanical design, automation, software, and IoT.
For questions, please contact: **longphan@ieee.org**

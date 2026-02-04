
# 🍲 Continuous OVercooked Simulator in Multi Agent Reinforcemnet Learning

This project implements a **multi-agent reinforcement learning** environment inspired by *Overcooked AI*.  
Two agents must cooperate implicitly (without pre-defined roles, they will chose their role by their own decesion based on environment feedback) to complete cooking tasks such as:
- Picking up 3 onions and one dish  
- Cooking onions as a soup in a pot  
- Delivering finished soup  with dish to a counter  


## 📦 Environment Setup


This project uses **Unity ML-Agents** package with **conda** envionment package management tool.

### 1. Create and activate a conda environment (About the python version, you can take reference from Unity Ml-Agents guideline. It's better to set up the environment step by step followed by Unity Ml-Agnets guide)

import **ContinousOVercookedSimulator.unitypackage** to your unity 3D project.

And then create you own environment by conda.
```bash
conda create -n overcooked
conda activate overcooked
````
---

## 🚀 Training

Run training with your configuration file (`run.yaml`):

```bash
mlagents-learn run.yaml --run-id=<your_run_id> --force
```

* `run.yaml` → defines PPO/MAPOCA training parameters
* `--run-id` → unique identifier for each experiment
* `--force` → overwrite previous runs with the same ID

---

## 📊 Monitoring with TensorBoard

Start TensorBoard to visualize training curves:

```bash
tensorboard --logdir results/<your_run_id>
```

You can monitor:

* **Cumulative Reward** → overall performance of agents
* **Completion Time** → efficiency of task completion

---

## 🤝 Cooperation

* Two agents interact in the same environment
* Role allocation is **implicit**, not pre-defined
* Agents must coordinate to achieve maximum reward by finishing and delivering cooked food

---

---

## 📜 Usage Recap
```bash
conda activate overcooked
mlagents-learn run.yaml --run-id=test_run --force
tensorboard --logdir results/test_run
```

```
```

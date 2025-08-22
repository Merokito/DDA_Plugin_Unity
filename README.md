# Dynamic Difficulty Adjustment (DDA) Unity Plugin

A modular Unity plugin for **Dynamic Difficulty Adjustment (DDA)**.  
It adapts game difficulty in real time based on player performance using either **fuzzy logic** (Mamdani-style) or **machine learning models** (ONNX Runtime).  

The plugin is designed to be **agnostic to the host game**, with a clean architecture based on interfaces and runtime metrics.  

---

## Features
- **Channel-based architecture**: expose and normalize gameplay metrics via `IChannel`.  
- **Dynamic adjustments**: apply difficulty deltas to gameplay elements via `IAdjustmentHandler`.  
- **Decision engines**:  
  - Fuzzy engine (`FuzzyDecisionEngine`) with rules and membership functions.  
  - ML engine (`MLDecisionEngine`) that loads ONNX regressors trained offline.  
- **Observability tools**:  
  - `MetricSeries` for storing metric histories.  
  - `MetricsDashboard` for real-time visualization and CSV export.  
- **Testing suite**: NUnit-based tests integrated with Unity Test Runner.  

---

## Installation

1. Open your Unity project.  
2. Add the package to `Packages/manifest.json`:
   ```json
   {
     "dependencies": {
          "com.kevinquarenghi.dda": "https://github.com/Merokito/DDA_Plugin_Unity.git?path=com.kevinquarenghi.dda"
     }
   }
   ```
3. Unity will download and import the plugin automatically.  

---

## Usage

1. **Expose metrics**:  
   Implement `IChannel` in your gameplay classes (e.g. `PlayerController`, `EnemySpawner`) to provide normalized values like Health, Experience, SpawnRate, etc.  

2. **Apply adjustments**:  
   Implement `IAdjustmentHandler` in the same or other classes to receive difficulty deltas (e.g. adjust spawn count, tweak damage, scale enemy tiers).  

3. **Configure decision engine**:  
   - Use `FuzzyDecisionEngine` with fuzzy rules defined in the editor.  
   - Or provide `.onnx` models exported via the [DDA ML Pipeline](https://github.com/<your-username>/dda-ml-pipeline).  

4. **Monitor metrics**:  
   - Use `MetricSeries` to log metric values over time.  
   - Open `MetricsDashboard` in the Unity Editor to toggle metrics, customize colors, and export data to CSV.  

---

## Example

```csharp
public class EnemySpawner : MonoBehaviour, IChannel, IAdjustmentHandler
{
    public float spawnRate;
    public int spawnCount;

    // Expose metrics
    public IEnumerable<MetricData> GetMetrics()
    {
        yield return new MetricData("SpawnRate", spawnRate, 0f, 1f);
        yield return new MetricData("SpawnCount", spawnCount, 0f, 50f);
    }

    // Apply adjustments
    public void ApplyAdjustment(string metric, float delta)
    {
        if (metric == "SpawnRate") spawnRate += delta;
        if (metric == "SpawnCount") spawnCount += Mathf.RoundToInt(delta);
    }
}
```

---

## Development & Testing
- The plugin includes a `Tests/` folder with NUnit-based tests (fuzzy engine, ML engine, metric series).  
- Tests can be run directly from the Unity **Test Runner**.  
- ONNX Runtime binaries are embedded to ensure compatibility out of the box.  

---

## Roadmap
- Add reinforcement learning support.  
- Optimize runtime performance for mobile/VR targets.  
- Publish on the official Unity Asset Store.  
- Explore a twin project for Unreal Engine.  

## Related Repositories
- [DDA ML Pipeline](https://github.com/Merokito/dda-ml-pipeline): Python scripts for training and exporting ONNX models.  

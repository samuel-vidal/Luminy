import React, { useState, useMemo } from 'react';
import { Display, BoxPlot, AxesStyle } from 'luminy-react';

export const BoxPlotDemo: React.FC = () => {
  const [boxWidth, setBoxWidth] = useState<number>(0.65);
  const [sampleSize, setSampleSize] = useState<number>(80);
  const [treatmentDelta, setTreatmentDelta] = useState<number>(2.5);
  const [variance, setVariance] = useState<number>(1.0);
  const [showGrid, setShowGrid] = useState<boolean>(true);
  const [showLegend, setShowLegend] = useState<boolean>(true);
  const [axesStyle, setAxesStyle] = useState<AxesStyle>('frame');

  const { controlGroup, drugAGroup, drugBGroup } = useMemo(() => {
    const generateGroup = (offset: number, scale: number, seed: number) => {
      const arr: number[] = [];
      for (let i = 0; i < sampleSize; i++) {
        const u1 = ((Math.sin(i * 12.9898 + seed) * 43758.5453) % 1 + 1) % 1;
        const u2 = ((Math.sin(i * 78.233 + seed + 5) * 43758.5453) % 1 + 1) % 1;
        const r = Math.sqrt(-2.0 * Math.log(Math.max(1e-6, u1)));
        const theta = 2.0 * Math.PI * u2;
        const z = r * Math.cos(theta);

        // Add occasional intentional outlier
        let val = offset + z * scale * variance;
        if (i === 3) val += 4.5 * scale;
        if (i === 17) val -= 4.2 * scale;

        arr.push(val);
      }
      return arr;
    };

    return {
      controlGroup: generateGroup(10.0, 1.2, 101),
      drugAGroup: generateGroup(11.2, 1.4, 202),
      drugBGroup: generateGroup(10.0 + treatmentDelta, 1.1, 303)
    };
  }, [sampleSize, treatmentDelta, variance]);

  return (
    <div className="showcase-grid">
      <aside className="control-panel">
        <h3 className="panel-title">BoxPlot Controls</h3>

        <div className="control-group">
          <label>
            <span>Box Width</span>
            <span>{boxWidth.toFixed(2)} units</span>
          </label>
          <input
            type="range"
            min="0.25"
            max="0.85"
            step="0.05"
            value={boxWidth}
            onChange={(e) => setBoxWidth(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Cohort Size (N)</span>
            <span>{sampleSize} per cohort</span>
          </label>
          <input
            type="range"
            min="20"
            max="180"
            step="10"
            value={sampleSize}
            onChange={(e) => setSampleSize(parseInt(e.target.value, 10))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Treatment Delta (Δ)</span>
            <span>+{treatmentDelta.toFixed(1)}</span>
          </label>
          <input
            type="range"
            min="0.0"
            max="6.0"
            step="0.2"
            value={treatmentDelta}
            onChange={(e) => setTreatmentDelta(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Dispersion (σ)</span>
            <span>{variance.toFixed(2)}x</span>
          </label>
          <input
            type="range"
            min="0.5"
            max="2.0"
            step="0.1"
            value={variance}
            onChange={(e) => setVariance(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>Axes Style</label>
          <select value={axesStyle} onChange={(e) => setAxesStyle(e.target.value as AxesStyle)}>
            <option value="frame">Frame (Scientific)</option>
            <option value="normal">Normal (Center 0,0)</option>
            <option value="none">None (Clean)</option>
          </select>
        </div>

        <div className="control-group">
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showGrid}
              onChange={(e) => setShowGrid(e.target.checked)}
            />
            Show Grid
          </label>
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showLegend}
              onChange={(e) => setShowLegend(e.target.checked)}
            />
            Show Legend
          </label>
        </div>
      </aside>

      <section className="chart-card">
        <Display
          width={720}
          height={460}
          title="Clinical Trial Cohort Comparison (Tukey Boxplot)"
          xLabel="Cohort Group (1: Control, 2: Dose A, 3: Dose B)"
          yLabel="Biomarker Level (mg/dL)"
          showGrid={showGrid}
          showLegend={showLegend}
          axesStyle={axesStyle}
        >
          <BoxPlot
            data={controlGroup}
            x={1}
            width={boxWidth}
            color="#0284c7"
            label="Placebo Control"
          />
          <BoxPlot
            data={drugAGroup}
            x={2}
            width={boxWidth}
            color="#10b981"
            label="Treatment Alpha"
          />
          <BoxPlot
            data={drugBGroup}
            x={3}
            width={boxWidth}
            color="#f59e0b"
            label="Treatment Beta"
          />
        </Display>

        <div className="stats-bar">
          <span>Cohorts: <strong>3 Distinct Groups</strong></span>
          <span>Sample per Cohort: <strong>{sampleSize}</strong></span>
          <span>Method: <strong>Tukey 1.5×IQR with Outlier Dots</strong></span>
        </div>
      </section>
    </div>
  );
};

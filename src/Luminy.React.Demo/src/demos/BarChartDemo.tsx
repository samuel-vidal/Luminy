import React, { useState, useMemo } from 'react';
import { Display, BarChart, AxesStyle } from 'luminy-react';

export const BarChartDemo: React.FC = () => {
  const [barCount, setBarCount] = useState<number>(6);
  const [barWidth, setBarWidth] = useState<number>(0.7);
  const [includeNegative, setIncludeNegative] = useState<boolean>(false);
  const [growthFactor, setGrowthFactor] = useState<number>(1.2);
  const [showGrid, setShowGrid] = useState<boolean>(true);
  const [showLegend, setShowLegend] = useState<boolean>(true);
  const [axesStyle, setAxesStyle] = useState<AxesStyle>('frame');

  const { heights, xIndices, labels } = useMemo(() => {
    const defaultLabels = ['Q1', 'Q2', 'Q3', 'Q4', 'Q5', 'Q6', 'Q7', 'Q8', 'Q9', 'Q10'];
    const h: number[] = [];
    const x: number[] = [];
    const l: string[] = [];

    for (let i = 0; i < barCount; i++) {
      x.push(i + 1);
      l.push(defaultLabels[i] || `P${i + 1}`);

      // Synthetic quarterly profile
      const base = 25 + Math.pow(i, growthFactor) * 8;
      const variation = Math.sin(i * 1.7) * 15;
      let val = Math.round(base + variation);

      if (includeNegative && i % 3 === 2) {
        val = -Math.round(val * 0.4);
      }

      h.push(val);
    }

    return { heights: h, xIndices: x, labels: l };
  }, [barCount, barWidth, includeNegative, growthFactor]);

  const sumTotal = heights.reduce((a, b) => a + b, 0);

  return (
    <div className="showcase-grid">
      <aside className="control-panel">
        <h3 className="panel-title">BarChart Controls</h3>

        <div className="control-group">
          <label>
            <span>Number of Bars</span>
            <span>{barCount}</span>
          </label>
          <input
            type="range"
            min="3"
            max="10"
            step="1"
            value={barCount}
            onChange={(e) => setBarCount(parseInt(e.target.value, 10))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Bar Width Ratio</span>
            <span>{barWidth.toFixed(2)}</span>
          </label>
          <input
            type="range"
            min="0.2"
            max="0.95"
            step="0.05"
            value={barWidth}
            onChange={(e) => setBarWidth(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Growth Profile</span>
            <span>{growthFactor.toFixed(1)}x</span>
          </label>
          <input
            type="range"
            min="0.5"
            max="2.0"
            step="0.1"
            value={growthFactor}
            onChange={(e) => setGrowthFactor(parseFloat(e.target.value))}
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
              checked={includeNegative}
              onChange={(e) => setIncludeNegative(e.target.checked)}
            />
            Include Deficit / Negative Quarters
          </label>
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
          title="Quarterly Performance & Financial Metrics"
          xLabel="Fiscal Period"
          yLabel="Net Earnings (k€)"
          showGrid={showGrid}
          showLegend={showLegend}
          axesStyle={axesStyle}
        >
          <BarChart
            x={xIndices}
            heights={heights}
            barWidth={barWidth}
            color="#0d9488"
            label="Net Results (k€)"
          />
        </Display>

        <div className="stats-bar">
          <span>Categories: <strong>{labels.join(', ')}</strong></span>
          <span>Net Cumulative: <strong>{sumTotal} k€</strong></span>
          <span>Bar Width: <strong>{(barWidth * 100).toFixed(0)}% slot</strong></span>
        </div>
      </section>
    </div>
  );
};

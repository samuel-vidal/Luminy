import React, { useState, useMemo } from 'react';
import { Display, LinePlot, AxesStyle } from 'luminy-react';

export const LinePlotDemo: React.FC = () => {
  const [frequency, setFrequency] = useState<number>(1.8);
  const [amplitude, setAmplitude] = useState<number>(1.4);
  const [noiseLevel, setNoiseLevel] = useState<number>(0.3);
  const [samples, setSamples] = useState<number>(160);
  const [showGrid, setShowGrid] = useState<boolean>(true);
  const [showLegend, setShowLegend] = useState<boolean>(true);
  const [showCos, setShowCos] = useState<boolean>(true);
  const [axesStyle, setAxesStyle] = useState<AxesStyle>('frame');

  const { xData, sinData, cosData, noisyData } = useMemo(() => {
    const x: number[] = [];
    const sin: number[] = [];
    const cos: number[] = [];
    const noisy: number[] = [];

    const step = (4 * Math.PI) / samples;
    for (let i = 0; i <= samples; i++) {
      const t = i * step;
      x.push(t);
      const pureSin = amplitude * Math.sin(frequency * t);
      const pureCos = amplitude * Math.cos(frequency * t);
      const noise = (Math.sin(i * 12.9898) * 43758.5453) % 1;
      sin.push(pureSin);
      cos.push(pureCos);
      noisy.push(pureSin + (noise - 0.5) * 2 * noiseLevel);
    }

    return { xData: x, sinData: sin, cosData: cos, noisyData: noisy };
  }, [frequency, amplitude, noiseLevel, samples]);

  return (
    <div className="showcase-grid">
      <aside className="control-panel">
        <h3 className="panel-title">LinePlot Controls</h3>

        <div className="control-group">
          <label>
            <span>Frequency (ω)</span>
            <span>{frequency.toFixed(1)} rad/s</span>
          </label>
          <input
            type="range"
            min="0.5"
            max="6.0"
            step="0.1"
            value={frequency}
            onChange={(e) => setFrequency(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Amplitude (A)</span>
            <span>{amplitude.toFixed(2)}</span>
          </label>
          <input
            type="range"
            min="0.2"
            max="3.0"
            step="0.1"
            value={amplitude}
            onChange={(e) => setAmplitude(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Noise Level (σ)</span>
            <span>{noiseLevel.toFixed(2)}</span>
          </label>
          <input
            type="range"
            min="0.0"
            max="1.0"
            step="0.05"
            value={noiseLevel}
            onChange={(e) => setNoiseLevel(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Sample Points (N)</span>
            <span>{samples}</span>
          </label>
          <input
            type="range"
            min="20"
            max="300"
            step="10"
            value={samples}
            onChange={(e) => setSamples(parseInt(e.target.value, 10))}
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
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showCos}
              onChange={(e) => setShowCos(e.target.checked)}
            />
            Show Cosine Reference
          </label>
        </div>
      </aside>

      <section className="chart-card">
        <Display
          width={720}
          height={460}
          title={`Harmonic Oscillator (${frequency.toFixed(1)} rad/s)`}
          xLabel="Time t (s)"
          yLabel="Signal Amplitude"
          showGrid={showGrid}
          showLegend={showLegend}
          axesStyle={axesStyle}
        >
          <LinePlot
            x={xData}
            y={noisyData}
            label="Noisy Measurement"
            color="#94a3b8"
            strokeWidth={1.5}
          />
          <LinePlot
            x={xData}
            y={sinData}
            label="Pure Sine"
            color="#0284c7"
            strokeWidth={2.5}
          />
          {showCos && (
            <LinePlot
              x={xData}
              y={cosData}
              label="Cosine Phase Ref"
              color="#f97316"
              strokeWidth={2}
              strokeDasharray="4 4"
            />
          )}
        </Display>

        <div className="stats-bar">
          <span>Points: <strong>{samples}</strong></span>
          <span>Period: <strong>{((2 * Math.PI) / frequency).toFixed(2)}s</strong></span>
          <span>Rendering: <strong>Pure SVG Virtual DOM (60 FPS)</strong></span>
        </div>
      </section>
    </div>
  );
};

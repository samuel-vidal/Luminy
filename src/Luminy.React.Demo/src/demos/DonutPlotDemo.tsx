import React, { useState } from 'react';
import { Display, DonutPlot } from 'luminy-react';

export const DonutPlotDemo: React.FC = () => {
  const [desktop, setDesktop] = useState<number>(45);
  const [mobile, setMobile] = useState<number>(30);
  const [tablet, setTablet] = useState<number>(18);
  const [consoleCount, setConsoleCount] = useState<number>(7);
  const [internalRadius, setInternalRadius] = useState<number>(0.6);
  const [externalRadius, setExternalRadius] = useState<number>(1.0);
  const [insideDisplay, setInsideDisplay] = useState<'percent' | 'raw' | 'none'>('percent');
  const [showSliceLabels, setShowSliceLabels] = useState<boolean>(true);
  const [showLegend, setShowLegend] = useState<boolean>(true);

  const values = [desktop, mobile, tablet, consoleCount];
  const labels = ['Desktop', 'Mobile', 'Tablet', 'Console'];

  return (
    <div className="showcase-grid">
      <aside className="control-panel">
        <h3 className="panel-title">Donut &amp; Pie Controls</h3>

        <div className="control-group">
          <label>
            <span>Hole Radius (Internal)</span>
            <span>{internalRadius === 0 ? '0 (Full Pie)' : internalRadius.toFixed(2)}</span>
          </label>
          <input
            type="range"
            min="0.0"
            max="0.85"
            step="0.05"
            value={internalRadius}
            onChange={(e) => setInternalRadius(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>
            <span>Outer Radius (External)</span>
            <span>{externalRadius.toFixed(2)}</span>
          </label>
          <input
            type="range"
            min="0.6"
            max="1.2"
            step="0.05"
            value={externalRadius}
            onChange={(e) => setExternalRadius(parseFloat(e.target.value))}
          />
        </div>

        <div className="control-group">
          <label>Inside Number Display</label>
          <select value={insideDisplay} onChange={(e) => setInsideDisplay(e.target.value as any)}>
            <option value="percent">Percentage (%)</option>
            <option value="raw">Raw Value</option>
            <option value="none">None</option>
          </select>
        </div>

        <div className="control-group">
          <label><span>Desktop Slice</span><span>{desktop}</span></label>
          <input type="range" min="5" max="100" value={desktop} onChange={(e) => setDesktop(parseInt(e.target.value, 10))} />
        </div>

        <div className="control-group">
          <label><span>Mobile Slice</span><span>{mobile}</span></label>
          <input type="range" min="5" max="100" value={mobile} onChange={(e) => setMobile(parseInt(e.target.value, 10))} />
        </div>

        <div className="control-group">
          <label><span>Tablet Slice</span><span>{tablet}</span></label>
          <input type="range" min="2" max="60" value={tablet} onChange={(e) => setTablet(parseInt(e.target.value, 10))} />
        </div>

        <div className="control-group">
          <label><span>Console Slice</span><span>{consoleCount}</span></label>
          <input type="range" min="1" max="40" value={consoleCount} onChange={(e) => setConsoleCount(parseInt(e.target.value, 10))} />
        </div>

        <div className="control-group">
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showSliceLabels}
              onChange={(e) => setShowSliceLabels(e.target.checked)}
            />
            Show Outside Labels (110% Radius)
          </label>
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showLegend}
              onChange={(e) => setShowLegend(e.target.checked)}
            />
            Show Legend Box
          </label>
        </div>
      </aside>

      <section className="chart-card">
        <Display
          width={720}
          height={460}
          title={internalRadius === 0 ? 'Platform Market Share (Pie Chart)' : 'Platform Market Share (Donut)'}
          axesStyle="none"
          showGrid={false}
          showLegend={showLegend}
        >
          <DonutPlot
            values={values}
            labels={labels}
            externalRadius={externalRadius}
            internalRadius={internalRadius}
            insideValueDisplay={insideDisplay}
            showSliceLabels={showSliceLabels}
            minInsideValueThreshold={0.04}
          />
        </Display>

        <div className="stats-bar">
          <span>Mode: <strong>{internalRadius === 0 ? 'Pie Chart' : `Donut (${Math.round((internalRadius / externalRadius) * 100)}% hole)`}</strong></span>
          <span>Total Units: <strong>{values.reduce((a, b) => a + b, 0)}</strong></span>
          <span>Slices: <strong>{values.length} Categories</strong></span>
        </div>
      </section>
    </div>
  );
};

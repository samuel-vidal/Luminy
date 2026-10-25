import React, { useState } from 'react';
import { LinePlotDemo } from './demos/LinePlotDemo';
import { DonutPlotDemo } from './demos/DonutPlotDemo';
import { PointPlotDemo } from './demos/PointPlotDemo';
import { BarChartDemo } from './demos/BarChartDemo';
import { HistogramPlotDemo } from './demos/HistogramPlotDemo';
import { BoxPlotDemo } from './demos/BoxPlotDemo';
import { MatrixPlotDemo } from './demos/MatrixPlotDemo';

export type DemoTab = 'line' | 'donut' | 'point' | 'bar' | 'histogram' | 'box' | 'matrix';

interface TabItem {
  id: DemoTab;
  name: string;
  icon: string;
  badge?: string;
  description: string;
}

const TABS: TabItem[] = [
  {
    id: 'line',
    name: 'LinePlot',
    icon: '📈',
    badge: 'Continuous',
    description: 'Signals, waveforms & time-series curves'
  },
  {
    id: 'donut',
    name: 'DonutPlot',
    icon: '🍩',
    badge: 'Radial',
    description: 'Proportional distributions, pie & donut charts'
  },
  {
    id: 'point',
    name: 'PointPlot',
    icon: '🔵',
    badge: 'Scatter',
    description: 'Discrete coordinate scatter & multi-cluster plots'
  },
  {
    id: 'bar',
    name: 'BarChart',
    icon: '📊',
    badge: 'Categorical',
    description: 'Discrete series, bars & financial histograms'
  },
  {
    id: 'histogram',
    name: 'HistogramPlot',
    icon: '📶',
    badge: 'Distribution',
    description: 'Frequency bins & statistical densities'
  },
  {
    id: 'box',
    name: 'BoxPlot',
    icon: '📦',
    badge: 'Summary',
    description: 'Tukey whiskers, quartiles & outlier detection'
  },
  {
    id: 'matrix',
    name: 'MatrixPlot',
    icon: '🔲',
    badge: 'Heatmap',
    description: '2D matrix intensity & spatial fields'
  }
];

export const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<DemoTab>('line');

  return (
    <div className="app-container">
      <header className="header">
        <div className="header-brand">
          <h1>
            Luminy.React <span className="badge">Interactive Showcase</span>
          </h1>
          <p className="subtitle">
            Pure vector mathematical visualization in React — Zero external graphics runtime.
          </p>
        </div>

        {/* Tab Navigation */}
        <nav className="tab-nav" aria-label="Plot Type Selector">
          {TABS.map((tab) => {
            const isActive = activeTab === tab.id;
            return (
              <button
                key={tab.id}
                type="button"
                className={`tab-button ${isActive ? 'active' : ''}`}
                onClick={() => setActiveTab(tab.id)}
              >
                <span className="tab-icon">{tab.icon}</span>
                <div className="tab-info">
                  <div className="tab-header">
                    <span className="tab-name">{tab.name}</span>
                    {tab.badge && <span className="tab-badge">{tab.badge}</span>}
                  </div>
                  <span className="tab-desc">{tab.description}</span>
                </div>
              </button>
            );
          })}
        </nav>
      </header>

      {/* Main Content Area: Render active demo */}
      <main className="tab-content">
        {activeTab === 'line' && <LinePlotDemo />}
        {activeTab === 'donut' && <DonutPlotDemo />}
        {activeTab === 'point' && <PointPlotDemo />}
        {activeTab === 'bar' && <BarChartDemo />}
        {activeTab === 'histogram' && <HistogramPlotDemo />}
        {activeTab === 'box' && <BoxPlotDemo />}
        {activeTab === 'matrix' && <MatrixPlotDemo />}
      </main>
    </div>
  );
};

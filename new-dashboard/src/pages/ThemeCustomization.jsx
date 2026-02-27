import React, { useState } from 'react';
import Layout from '../components/Layout';
import { useTheme } from '../contexts/ThemeContext';
import { toast } from 'react-toastify';

const ThemeCustomization = () => {
  const { isDarkMode, setDarkMode } = useTheme();
  const [customColors, setCustomColors] = useState(() => {
    const saved = localStorage.getItem('customThemeColors');
    return saved ? JSON.parse(saved) : {
      primary: '#5e72e4',
      secondary: '#8392ab',
      success: '#2dce89',
      danger: '#f5365c',
      warning: '#fb6340',
      info: '#11cdef',
    };
  });

  const handleColorChange = (colorName, value) => {
    const newColors = { ...customColors, [colorName]: value };
    setCustomColors(newColors);
    localStorage.setItem('customThemeColors', JSON.stringify(newColors));
    applyCustomColors(newColors);
    toast.success('Theme color updated!');
  };

  const applyCustomColors = (colors) => {
    const root = document.documentElement;
    Object.keys(colors).forEach((key) => {
      root.style.setProperty(`--${key}-color`, colors[key]);
    });
  };

  const resetToDefault = () => {
    const defaultColors = {
      primary: '#5e72e4',
      secondary: '#8392ab',
      success: '#2dce89',
      danger: '#f5365c',
      warning: '#fb6340',
      info: '#11cdef',
    };
    setCustomColors(defaultColors);
    localStorage.setItem('customThemeColors', JSON.stringify(defaultColors));
    applyCustomColors(defaultColors);
    toast.success('Theme colors reset to default!');
  };

  React.useEffect(() => {
    applyCustomColors(customColors);
  }, []);

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0">Theme Customization</h4>
          </div>
        </div>
      </div>

      <div className="row">
        <div className="col-lg-6">
          <div className="card">
            <div className="card-header">
              <h5 className="card-title mb-0">Color Scheme</h5>
            </div>
            <div className="card-body">
              <div className="mb-3">
                <label className="form-label">Dark Mode</label>
                <div className="form-check form-switch">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    checked={isDarkMode}
                    onChange={(e) => setDarkMode(e.target.checked)}
                  />
                  <label className="form-check-label">
                    {isDarkMode ? 'Enabled' : 'Disabled'}
                  </label>
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label">Primary Color</label>
                <div className="d-flex align-items-center">
                  <input
                    type="color"
                    className="form-control form-control-color me-2"
                    value={customColors.primary}
                    onChange={(e) => handleColorChange('primary', e.target.value)}
                  />
                  <input
                    type="text"
                    className="form-control"
                    value={customColors.primary}
                    onChange={(e) => handleColorChange('primary', e.target.value)}
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label">Secondary Color</label>
                <div className="d-flex align-items-center">
                  <input
                    type="color"
                    className="form-control form-control-color me-2"
                    value={customColors.secondary}
                    onChange={(e) => handleColorChange('secondary', e.target.value)}
                  />
                  <input
                    type="text"
                    className="form-control"
                    value={customColors.secondary}
                    onChange={(e) => handleColorChange('secondary', e.target.value)}
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label">Success Color</label>
                <div className="d-flex align-items-center">
                  <input
                    type="color"
                    className="form-control form-control-color me-2"
                    value={customColors.success}
                    onChange={(e) => handleColorChange('success', e.target.value)}
                  />
                  <input
                    type="text"
                    className="form-control"
                    value={customColors.success}
                    onChange={(e) => handleColorChange('success', e.target.value)}
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label">Danger Color</label>
                <div className="d-flex align-items-center">
                  <input
                    type="color"
                    className="form-control form-control-color me-2"
                    value={customColors.danger}
                    onChange={(e) => handleColorChange('danger', e.target.value)}
                  />
                  <input
                    type="text"
                    className="form-control"
                    value={customColors.danger}
                    onChange={(e) => handleColorChange('danger', e.target.value)}
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label">Warning Color</label>
                <div className="d-flex align-items-center">
                  <input
                    type="color"
                    className="form-control form-control-color me-2"
                    value={customColors.warning}
                    onChange={(e) => handleColorChange('warning', e.target.value)}
                  />
                  <input
                    type="text"
                    className="form-control"
                    value={customColors.warning}
                    onChange={(e) => handleColorChange('warning', e.target.value)}
                  />
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label">Info Color</label>
                <div className="d-flex align-items-center">
                  <input
                    type="color"
                    className="form-control form-control-color me-2"
                    value={customColors.info}
                    onChange={(e) => handleColorChange('info', e.target.value)}
                  />
                  <input
                    type="text"
                    className="form-control"
                    value={customColors.info}
                    onChange={(e) => handleColorChange('info', e.target.value)}
                  />
                </div>
              </div>

              <button className="btn btn-secondary" onClick={resetToDefault}>
                Reset to Default
              </button>
            </div>
          </div>
        </div>

        <div className="col-lg-6">
          <div className="card">
            <div className="card-header">
              <h5 className="card-title mb-0">Preview</h5>
            </div>
            <div className="card-body">
              <div className="mb-3">
                <button className="btn btn-primary me-2">Primary Button</button>
                <button className="btn btn-secondary me-2">Secondary Button</button>
                <button className="btn btn-success me-2">Success Button</button>
                <button className="btn btn-danger me-2">Danger Button</button>
                <button className="btn btn-warning me-2">Warning Button</button>
                <button className="btn btn-info">Info Button</button>
              </div>

              <div className="alert alert-primary" role="alert">
                Primary alert message
              </div>
              <div className="alert alert-success" role="alert">
                Success alert message
              </div>
              <div className="alert alert-danger" role="alert">
                Danger alert message
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default ThemeCustomization;


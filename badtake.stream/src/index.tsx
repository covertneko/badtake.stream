import React, { useState } from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App, { ThemeContext } from './App';
import * as serviceWorker from './serviceWorker';
import { ThemeProvider } from '@material-ui/styles';
import { CssBaseline, createMuiTheme } from '@material-ui/core';
import { ApplicationInsights } from '@microsoft/applicationinsights-web'
import config from './config';

const appInsights = new ApplicationInsights({ config: {
  instrumentationKey: config.applicationInsightsKey
} });
appInsights.loadAppInsights();
appInsights.trackPageView();

const ThemedApp: React.FC = (props) => {
    const [themeType, setThemeType] = useState('dark')
    
    let theme = createMuiTheme({
        palette: {
            type: themeType as any
        }
    })

    return (<ThemeContext.Provider value={{ themeType: theme.palette.type, setTheme: (t) => setThemeType(t) }}>
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <App />
        </ThemeProvider>
    </ThemeContext.Provider>)
}

ReactDOM.render(<ThemedApp />,
    document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();

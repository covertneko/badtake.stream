import React from 'react';
import './App.css';
import Feed from './components/Feed';

import * as signalR from '@microsoft/signalr'
import { Grid, createStyles, Theme, CssBaseline, useMediaQuery, createMuiTheme, useTheme } from '@material-ui/core';
import { makeStyles, ThemeProvider } from '@material-ui/core/styles';
import Metrics from './components/Metrics';

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/api/feed")
  .withAutomaticReconnect()
  .build()

connection.start().catch(err => console.error(err))

const useStyles = makeStyles((theme: Theme) => createStyles({
  root: {
  },
  metrics: {
    background: theme.palette.background.paper,
    'box-shadow': theme.shadows[2],
    'z-index': 2
  },
  feed: {
    background: theme.palette.type == 'dark' ? theme.palette.grey[900] : theme.palette.background.default,
    height: '100vh',
    [theme.breakpoints.down('xs')]: {
      height: 'auto',
      'min-height': `calc(100vh - ${theme.spacing(8)}px)`,
      'max-height': `calc(100vh - ${theme.spacing(8)}px)`
    }
  }
}))

export const ThemeContext = React.createContext({ themeType: 'dark', setTheme: (type: string) => { } })

const App = () => {
  const classes = useStyles()
  const theme = useTheme()
  const minimal = useMediaQuery(theme.breakpoints.down('xs'))

  return (
    <React.Fragment>
      <div className={classes.root}>
        <Grid container spacing={0} direction={minimal ? "column-reverse" : "row"}>
          <Grid item xs={12} md={7} className={classes.metrics}>
            <Metrics source={connection} />
          </Grid>
          <Grid item xs={12} md={5} className={classes.feed}>
            <Feed source={connection} />
          </Grid>
        </Grid>
      </div>
    </React.Fragment>
  );
}

export default App;

import React from "react";
import * as signalR from '@microsoft/signalr'
import numbro from 'numbro'
import { Typography, Card, CardContent, createStyles, makeStyles, Theme, withStyles, Grid, Link, Table, TableRow, TableBody, TableCell, TableHead, Paper, useMediaQuery, useTheme, IconButton, Switch, Chip, Icon } from "@material-ui/core";
import { ThemeContext } from "../App";
import { MatchModel } from "./Tweet";
import { OpenInNew } from '@material-ui/icons'

const styles = (theme: Theme) => createStyles({
  root: {
    padding: theme.spacing(3),
    background: theme.palette.background.default,
    height: '100%',
    display: 'flex',
    'flex-direction': 'column',

    '& > main': {
      'flex': '1 0',
      'height': '100%'
    },
    '& > footer': {
      'flex': '0 0'
    }
  },
  metrics: {
    'margin-top': theme.spacing(2),
    'margin-bottom': theme.spacing(2),
  },
  highScores: {
    'margin-top': theme.spacing(2),
    'margin-bottom': theme.spacing(2),
  },
  footer: {
    'width': '100%'
  },
  footerSettings: {
    'margin-left': 'auto'
  },
  metric: {
    height: '100%'
  },
  metricContent: {
    'text-align': 'center'
  }
})
const useStyles = makeStyles(styles)

const formatCount = (n: number) => numbro(n).format({ average: true, mantissa: 2 }).replace('.00', '') // trim trailing zeroes
const formatTitle = (n: number) => numbro(n).format({ thousandSeparated: true })

const Metric: React.FC<MetricProps> = (props) => {
  const classes = useStyles()
  const theme = useTheme()
  const small = useMediaQuery(theme.breakpoints.down('xs'))

  return (
    <Card className={classes.metric}>
      <CardContent className={classes.metricContent}>
        <Typography variant={small ? "h5" : "h4"} gutterBottom align="center" title={formatTitle(props.value)}>
          {formatCount(props.value)}
        </Typography>
        <Typography variant={small ? "subtitle2" : "subtitle1"} align="center">
          {props.name}
        </Typography>
      </CardContent>
    </Card>
  )
}

interface MetricProps {
  value: number,
  name: string
}

interface MetricsState {
  data: MetricsModel
}

interface MetricsModel {
  rate: number,
  totalToday: number,
  total: number,

  highScores: HighScore[]
}

export interface CurrentMetrics {
  recentMatches: MatchModel[]
  metrics: MetricsModel
}

class Metrics extends React.Component<MetricsProps, MetricsState> {
  constructor(props: MetricsProps) {
    super(props)

    this.state = {
      data: {
        rate: 0,
        totalToday: 0,
        total: 0,
        highScores: []
      },
    }
  }

  async componentDidMount() {
    this.props.source.on('updateMetrics', (data: MetricsModel) => {
      this.setState({ data })
    })

    // Load initial metrics data if no live updates have come in first
    let current: CurrentMetrics = await fetch('/api/metrics/current').then(r => r.json())
    if (this.state.data.total == 0 && current.metrics)
      this.setState({ data: current.metrics })
  }

  render() {
    const { classes } = this.props as any

    return <div className={classes.root}>
      <main>
        <section>
          <Typography variant="h5" gutterBottom>
            what?
          </Typography>

          <Typography gutterBottom>
            watch people "ok boomer" boomers on twitter in real time
          </Typography>
        </section>
        <section>
          <Typography variant="h5" gutterBottom>
            why?
          </Typography>

          <Typography gutterBottom>
            I still don't know
          </Typography>
        </section>

        <section>
          <Typography variant="h5" gutterBottom>
            give me juicy metrics
          </Typography>

          <Typography gutterBottom>
            ok
          </Typography>

          <Grid container spacing={2} className={classes.metrics}>
            <Grid item xs={12} sm={4}>
              <Metric value={this.state.data.rate} name="boomers ok'd per second" />
            </Grid>

            <Grid item xs={12} sm={4}>
              <Metric value={this.state.data.totalToday} name="boomers ok'd today" />
            </Grid>

            <Grid item xs={12} sm={4}>
              <Metric value={this.state.data.total} name="boomers ok'd since 2019-11-20" />
            </Grid>
          </Grid>
        </section>

        <section>
          <Typography variant="h5" gutterBottom>
            are there high scores?
          </Typography>

          <Typography gutterBottom>
            yes
          </Typography>

          <HighScores className={classes.highScores} items={this.state.data.highScores} />
        </section>
      </main>

      <ThemeContext.Consumer>
        {({ themeType, setTheme }) => (
          <Grid component="footer" container alignItems="center" className={classes.footer}>
            <Grid item>
              <Typography variant="caption">
                <Link href="https://twitter.com/covertneko" target="_blank" rel="noopener noreferrer" color="inherit">@covertneko</Link>
              </Typography>
            </Grid>
            <Grid item className={classes.footerSettings}>
              <Typography component="div" variant="caption">
                <Grid component="label" container alignItems="center" spacing={1}>
                  <Grid item>Dark</Grid>
                  <Grid item>
                    <Switch checked={themeType == 'light'} onChange={(_, c) => setTheme(c ? 'light' : 'dark')} />
                  </Grid>
                  <Grid item>Light</Grid>
                </Grid>
              </Typography>
            </Grid>
          </Grid>
        )}
      </ThemeContext.Consumer>
    </div>
  }
}

export interface MetricsProps {
  source: signalR.HubConnection
}

export default withStyles(styles)(Metrics)

const HighScores: React.FC<HighScoresProps> = (props) => {

  return (
    <Paper className={props.className}>
      <Table aria-label="high scores" size="small">
        <TableHead>
          <TableRow>
            <TableCell>
              boomer
            </TableCell>
            <TableCell>
              times ok'd
            </TableCell>
            <TableCell>
              top tweet
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {props.items.map(s => (<TableRow key={s.userId}>
            <TableCell>
              {s.userDisplayName}
            </TableCell>
            <TableCell title={formatTitle(s.topTweetCount)}>
              {formatCount(s.count)}
            </TableCell>
            <TableCell>
              <Chip
                href={s.topTweetUrl}
                target="_blank"
                rel="noreferrer nofollow"
                component="a"
                clickable
                label={formatCount(s.topTweetCount)}
                title={formatTitle(s.topTweetCount)}
                icon={<OpenInNew />} />
            </TableCell>
          </TableRow>))}
        </TableBody>
      </Table>
    </Paper>
  )
}

interface HighScore {
  userId: number,
  userDisplayName: string,
  count: number,
  topTweetUrl: string,
  topTweetCount: number
}

interface HighScoresProps {
  className?: string,
  items: HighScore[]
}
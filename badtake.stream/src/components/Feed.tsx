import React from "react";
import * as signalR from '@microsoft/signalr'
import { withStyles } from '@material-ui/core/styles'
import Tweet, { MatchModel } from "./Tweet"
import { Grid, CircularProgress } from "@material-ui/core"
import { TransitionGroup, CSSTransition } from 'react-transition-group'
import TwitterIcon from "@material-ui/icons/Twitter"
import { CurrentMetrics } from "./Metrics";

interface FeedState {
    items: MatchModel[]
}

interface FeedProps {
    source: signalR.HubConnection
}

const useStyles = withStyles(theme => ({
  root: {
      height: '100%',
      'overflow-y': 'scroll',
      padding: theme.spacing(4),
      'box-sizing': 'border-box',
      display: 'flex',
      'flex-direction': 'column',
      'align-items': 'center',
      'position': 'relative',

      [theme.breakpoints.down('xs')]: {
          'overflow-y': 'hidden',
        padding: theme.spacing(2)
      }
  },
  feed: {
      'margin-top': theme.spacing(3),
  },
  feedWrapper: {
      'max-width': '100%'
  },
  progress: {
      position: 'absolute',
      display: 'flex',
      'align-items': 'center',
      top: theme.spacing(1),
      left: theme.spacing(3)
  },
  twitterLogo: {
    color: "#1DA1F3",
    'margin-right': theme.spacing(1)
  },
  tweetEnter: {
      opacity: 0.1,
      transform: 'scale(0.2)'
  },
  tweetEnterActive: {
      opacity: 1,
      transform: 'scale(1)',
      transition: '0.4s opacity ease-out, 0.3s transform ease-out'
  }
}))

class Feed extends React.Component<FeedProps, FeedState> {
    constructor(props: Readonly<FeedProps>) {
        super(props)
        this.state = { items: [] }
    }

    async componentDidMount() {
        this.props.source.on('addMatch', (match: MatchModel) => {
            // Only display the last 8
            this.setState(prev => ({ items: [match, ...prev.items].slice(0, 8) }))
        })

        // Load initial feed data if no live updates have come in first
        // TODO: remove duplicate request from metrics. this is silly.
        let current: CurrentMetrics = await fetch('/api/metrics/current').then(r => r.json())
        if (this.state.items.length == 0 && current.recentMatches)
            this.setState({ items: current.recentMatches })
    }

    render() {
        const { classes } = this.props as any

        const items = this.state.items.map(t => <CSSTransition key={t.source.id}
            timeout={100}
            classNames={{
                enter: classes.tweetEnter,
                enterActive: classes.tweetEnterActive
            }}
        >
            <Grid xs={12} item>
                <Tweet data={t} />
            </Grid>
        </CSSTransition>)

        return <div className={classes.root}>
            <div className={classes.progress}>
                <TwitterIcon className={classes.twitterLogo}></TwitterIcon>
                <CircularProgress size="1em" />
            </div>

            <Grid container spacing={2} className={classes.feed}>
                <TransitionGroup className={classes.feedWrapper}>
                    {items}
                </TransitionGroup>
            </Grid>
        </div>
    }
}

export default useStyles(Feed)
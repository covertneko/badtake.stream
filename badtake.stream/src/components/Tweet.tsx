import React from "react";
import moment from "moment"
import { Avatar, Typography, Paper, Grid, makeStyles, createStyles, Theme } from "@material-ui/core";

const useStyles = makeStyles((theme: Theme) => {
    const replyColor = theme.palette.type == 'dark' ? theme.palette.grey[700] : theme.palette.grey[100]

    return createStyles({
        root: {
            'margin-bottom': theme.spacing(2)
        },
        paper: {
            padding: theme.spacing(1),
            overflow: 'hidden'
        },
        reply: {
        },
        replyPaper: {
            padding: theme.spacing(1),
            overflow: 'hidden',
            background: replyColor,
            color: theme.palette.getContrastText(replyColor),
            'margin-left': `${theme.spacing(4)}px`,
        },
        author: {
            'max-width': '30%',
            'border-right': `1px solid ${theme.palette.divider}`,

            '& > a': {
                display: 'flex',
                color: 'inherit',
                overflow: 'hidden',
                'align-items': 'center',
                'text-decoration': 'none'
            }
        },

        authorAvatar: {
            'margin-right': theme.spacing(1)
        },
        authorText: {
            overflow: 'hidden',
            '& .MuiTypography-noWrap': {
                display: 'block',
                'max-width': '100%',
                'text-overflow': 'ellipsis',
            }
        },

        content: {
            '& > a': {
                display: 'block',
                color: 'inherit',
                'text-decoration': 'none'
            }
        }
    })
})

const Tweet: React.FC<TweetProps> = (props) => {
    const classes = useStyles()

    return (<Paper className={props.className}>
        <Grid container wrap="nowrap" spacing={3}>
            <Grid item className={classes.author}>
                <a href={props.data.profileUrl} target="_blank" rel="noopener noreferrer" title={props.data.displayName}>
                    <Avatar alt={props.data.username} src={props.data.avatarUrl} className={classes.authorAvatar} />
                    <div className={classes.authorText}>
                        <Typography noWrap variant="subtitle2">
                            {props.data.displayName}
                        </Typography>
                        <Typography noWrap variant="caption">
                            @{props.data.username}
                        </Typography>
                    </div>
                </a>
            </Grid>
            <Grid item className={classes.content}>
                <a href={props.data.url} target="_blank" rel="noopener noreferrer">
                    <Typography variant="body2">{props.data.text}</Typography>
                    <Typography variant="caption" align="right">{moment(props.data.createdAt).fromNow()}</Typography>
                </a>
            </Grid>
        </Grid>
    </Paper>
    )
}

const Match: React.FC<MatchProps> = (props) => {
    const classes = useStyles()

    return (<Grid container spacing={1} className={classes.root}>
        <Grid item xs={12}>
            <Tweet className={classes.paper} data={props.data.target} />
        </Grid>

        <Grid item xs={12} className={classes.reply}>
            <Tweet className={classes.replyPaper} data={props.data.source} />
        </Grid>
   </Grid>)
}
export default Match

export interface TweetProps {
    data: TweetModel
    className?: string
}

export interface MatchProps {
    data: MatchModel
}

export interface TweetModel {
    id: number,
    userId: number,
    username: string,
    displayName: string,
    avatarUrl: string,
    profileUrl: string,
    url: string,

    text: string,
    createdAt: Date,
}

export interface MatchModel {
    source: TweetModel,
    target: TweetModel
}
# [badtake.stream](https://badtake.stream)

Auto-updating feed of every time someone replies "ok boomer" on Twitter.

Components:
- **badtake.stream**:
    react-based web interface
- **BadTakeStream.Feeder**:
    streams incoming tweets, calculates and saves metrics, and publishes metric/tweet updates to redis
- **BadTakeStream.Api**:
    consumes incoming updates via redis and distributes them to connected clients to update the interface
- **BadTakeStream.Shared**:
    shared components for api and feeder projects
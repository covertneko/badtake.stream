
const dev = {
    api: 'http://localhost:8081/',
    applicationInsightsKey: '1c6d8702-6981-4d1c-b669-4638487743ef'
}

const prod = {
    api: 'http://localhost:8081/',
    applicationInsightsKey: 'a8a9f721-d848-4cfc-a3d1-90682383d70a'
}

const config = process.env.REACT_APP_STAGE === 'production'
  ? prod
  : dev;

export default config
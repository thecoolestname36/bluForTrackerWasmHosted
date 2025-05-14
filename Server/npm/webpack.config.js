const path = require('path');

module.exports = {
    experiments: {
        asyncWebAssembly: true,
    },
    mode: 'development',
    entry: './src/bluForTracker.js',
    output: {
        path: path.resolve(__dirname, '../wwwroot'),
        filename: 'webpack.bundle.js',
    }
}

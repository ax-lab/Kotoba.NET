const path = require('path')

const OUTPUT = path.resolve(__dirname, 'build')
const APP = './src/app.ts'

const config = {
	mode: 'development',
	module: {
		rules: [
			{
				test: /\.tsx?$/,
				use: {
					loader: 'swc-loader',
					options: {
						jsc: {
							target: 'es5',
						},
					},
				},
				exclude: /node_modules/,
			},
		],
	},
}

module.exports = (env, args) => {
	const { server = false } = env || {}
	const app = {
		entry: server ? ['webpack-dev-server/client', APP] : APP,
		devtool: args.mode == 'production' ? undefined : 'inline-source-map',
		output: {
			filename: 'app.js',
			path: OUTPUT,
			publicPath: '/',
		},
		devServer: {
			host: '0.0.0.0',
			port: 29899,
			proxy: {
				'/': {
					target: 'http://localhost:29802',
				},
			},
			devMiddleware: {
				writeToDisk: true,
			},
		},
	}

	return Object.assign({}, config, app)
}

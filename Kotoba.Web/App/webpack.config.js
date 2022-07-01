const path = require('path')

const OUTPUT = path.resolve(__dirname, 'build')
const APP = './src/index.ts'

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
							parser: {
								syntax: 'typescript',
								tsx: true,
							},
							transform: {
								react: {
									runtime: 'automatic',
									pragma: 'h',
									pragmaFrag: 'Fragment',
								},
							},
						},
					},
				},
				exclude: /node_modules/,
			},
			{
				test: /\.less$/,
				use: ['style-loader', 'css-loader', 'less-loader'],
			},
		],
	},
	resolve: {
		extensions: ['.tsx', '.ts', '.js'],
		alias: {
			react: 'preact/compat',
			'react-dom/test-utils': 'preact/test-utils',
			'react-dom': 'preact/compat', // Must be below test-utils
			'react/jsx-runtime': 'preact/jsx-runtime',
		},
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
		},
	}

	return Object.assign({}, config, app)
}

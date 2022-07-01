import 'preact/devtools'
import { h, render } from 'preact'
import App from './App'

import './css/main.less'

function init() {
	const loading = document.getElementById('loading')
	loading && loading.parentElement?.removeChild(loading)
	render(App(), document.getElementById('root')!)
}

console.log('INIT!')

init()

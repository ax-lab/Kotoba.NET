import 'preact/debug';

import { render } from 'preact';
import App from './App';

import './css/main.less';

import * as graphql from './api/graphql';

function init() {
	const loading = document.getElementById('loading');
	loading && loading.parentElement?.removeChild(loading);
	render(App(), document.getElementById('root')!);
}

console.log('INIT!');


init();

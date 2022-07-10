import { useEffect, useState } from 'preact/hooks';

import * as entries from '../api/entries';

type AsyncResult<T extends (...args: any) => Promise<any>> = T extends (...args: any) => Promise<infer X> ? X : never;

const ViewEntries = () => {
	const [data, setData] = useState<AsyncResult<typeof entries.list>>();
	const [error, setError] = useState('');

	useEffect(() => {
		entries
			.list({ limit: 500 })
			.then((data) => {
				setError('');
				setData(data);
			})
			.catch(() => setError('failed to load entries'));
	}, []);

	if (error) {
		return <div class="error-message">Error: {error}</div>;
	}

	if (!data) {
		return <div>Loading entries...</div>;
	}

	return (
		<div>
			<div>
				Loaded {data.items.length} of {data.total} entries in {data.elapsed.toFixed(1)} ms
			</div>
			<div>
				{data.items.map((x) => (
					<div>
						<h2>{x.text}</h2>#{x.position} ({x.id})
					</div>
				))}
			</div>
		</div>
	);
};

export default ViewEntries;

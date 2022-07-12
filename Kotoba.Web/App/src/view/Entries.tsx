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
						<h2>
							{x.text}
							<em style="margin-left: 2em; font-size: 0.8rem">
								#{x.position} — {x.id}
							</em>
						</h2>

						<div>{x.kanji.map((x) => x.text).join(', ')}</div>
						<div>{x.reading.map((x) => x.text).join(', ')}</div>
						<ul>
							{x.sense.map((x) => (
								<li>{x.glossary.map((x) => x.text).join(', ')}</li>
							))}
						</ul>
					</div>
				))}
			</div>
		</div>
	);
};

export default ViewEntries;

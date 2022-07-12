import { indent } from '../util/text';

export async function query<T>(text: string, vars?: Record<string, unknown>) {
	const query = {
		query: indent(text),
		variables: vars || null,
	};
	return fetch('/api/graphql', {
		method: 'POST',
		body: JSON.stringify(query),
		headers: {
			'Content-Type': 'application/json',
		},
	})
		.then((result) => result.json())
		.then(({ data, errors }) => {
			if (Array.isArray(errors) && errors.length) {
				const message = (errors as Array<{ message: string }>).map((x) => x.message).join(' / ');
				console.error('GraphQL:', message, {
					query: text,
					variables: vars || null,
					errors: errors as unknown[],
				});
				throw new Error(message);
			}
			return data as T;
		});
}

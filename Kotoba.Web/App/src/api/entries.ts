import { query } from './graphql';

export type Entry = {
	id: number;
	position: number;
	text: string;
};

export async function list({ limit = 0, offset = 0 } = {}) {
	const decl = [`$offset: Int!`];
	const args = [`offset: $offset`];
	const vars = { offset } as Record<string, number>;
	const start = performance.now();
	if (limit) {
		decl.push(`$limit: Int!`);
		args.push(`limit: $limit`);
		vars.limit = limit;
	}

	const text = `query (${decl.join(', ')}) {
		entries {
			total: count
			items: list(${args.join(', ')}) {
				id position text
			}
		}
	}`;

	const data = await query<{
		entries: {
			total: number;
			items: Array<Entry>;
		};
	}>(text, vars);

	const elapsed = performance.now() - start;
	return { ...data.entries, elapsed };
}

export async function count() {
	const data = await query<{ entries: { count: number } }>(`{
		entries { count }
	}`);
	return data.entries.count;
}

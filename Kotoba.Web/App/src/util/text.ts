/**
 * Split the input string by lines.
 */
export function lines(input: string) {
	return (input && input.split(/\r\n?|\n/g)) || [];
}

export function indent(text: string) {
	const output = lines(text);
	while (output.length && !output[0].trim()) {
		output.shift();
	}
	while (output.length && !output[output.length - 1].trim()) {
		output.pop();
	}

	const indent = (output.length && (output[0].match(/^\s*/) ?? [])[0]) || '';
	const dedent = (line: string) => (line.startsWith(indent) ? line.slice(indent.length) : line);
	const indented = indent ? output.map(dedent) : output;

	return indented.map((x) => x.trimEnd()).join('\n');
}

import { describe, expect, it } from '@jest/globals';

import * as text from './text';

describe('text', () => {
	describe('lines', () => {
		it('splits by line', () => {
			expect(text.lines('a\nb\nc')).toEqual(['a', 'b', 'c']);
		});

		it('returns single line', () => {
			expect(text.lines('abc')).toEqual(['abc']);
		});

		it('splits by CRLF', () => {
			expect(text.lines('a\r\nb\r\nc')).toEqual(['a', 'b', 'c']);
		});

		it('splits by CR', () => {
			expect(text.lines('a\rb\rc')).toEqual(['a', 'b', 'c']);
		});

		it('returns empty for empty string', () => {
			expect(text.lines('')).toEqual([]);
		});
	});

	describe('indent', () => {
		it('should return plain input', () => {
			expect(text.indent('abc')).toEqual('abc');
		});

		it('should return empty input', () => {
			expect(text.indent('')).toEqual('');
		});

		it('should strip leading empty lines', () => {
			expect(text.indent('\nabc')).toEqual('abc');
			expect(text.indent('\n\nabc')).toEqual('abc');
			expect(text.indent('\n  \n\t\nabc')).toEqual('abc');
		});

		it('should strip trailing empty lines', () => {
			expect(text.indent('abc\n')).toEqual('abc');
			expect(text.indent('abc\n\n')).toEqual('abc');
			expect(text.indent('abc\n  \n\t\n')).toEqual('abc');
		});

		it('should normalize line break', () => {
			expect(text.indent('a\rb\r\nc')).toEqual('a\nb\nc');
		});

		it('should strip trailing indentation from first line', () => {
			expect(text.indent('  a\n    b\n  c\nd')).toEqual('a\n  b\nc\nd');
		});

		it('should trim line trailing space', () => {
			expect(text.indent('a  \n  \n  b  \nc  ')).toEqual('a\n\n  b\nc');
		});
	});
});

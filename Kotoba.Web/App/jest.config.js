module.exports = {
	rootDir: '.',
	roots: ['<rootDir>/src'],

	transform: {
		'^.+\\.(t|j)sx?$': ['@swc/jest'],
	},

	collectCoverage: false,
	coverageDirectory: './build/coverage',
	coveragePathIgnorePatterns: ['\\\\node_modules\\\\'],
	coverageReporters: ['json', 'html'],
};

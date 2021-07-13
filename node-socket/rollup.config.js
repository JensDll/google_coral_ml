import { defineConfig } from 'rollup'
import typescript from '@rollup/plugin-typescript'
import { terser } from 'rollup-plugin-terser'

const watchConfig = defineConfig({
  input: 'src/main.ts',
  output: {
    file: 'bundle.js',
    format: 'esm'
  },
  plugins: [typescript()]
})

const buildConfig = defineConfig({
  input: 'src/main.ts',
  output: {
    file: 'dist/bundle.min.js',
    format: 'esm',
    plugins: [terser()]
  },
  plugins: [typescript()]
})

export default args => {
  if (args.watch) {
    return watchConfig
  } else if (args.build) {
    delete args.build
    return buildConfig
  }
}
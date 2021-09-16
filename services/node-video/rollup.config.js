import { defineConfig } from 'rollup'
import typescript from '@rollup/plugin-typescript'
import { terser } from 'rollup-plugin-terser'

export default args => {
  const plugins = [typescript()]

  const config = defineConfig({
    input: 'src/main.ts',
    output: {
      file: 'dist/bundle.js',
      format: 'esm'
    },
    plugins
  })

  delete args.build

  return config
}

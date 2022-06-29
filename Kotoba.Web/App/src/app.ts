import { Map } from 'immutable'

console.log('Hello world!!!')

console.log('Testing library...')

const map = Map<number, string>().set(1, 'one').set(2, 'two').set(3, 'three')
console.log(`${map.get(1)}, ${map.get(2)}, ${map.get(3)}...`)

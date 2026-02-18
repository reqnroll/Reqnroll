import { Given } from '@cucumber/fake-cucumber'

Given('a step', () => {})

Given('a failing step', () => {
  throw new Error('whoops')
})

Given('a pending step', () => {
  return 'pending'
})

Given('a skipped step', () => {
  return 'skipped'
})

Given('an ambiguous step', () => {})

Given('an ambiguous step', () => {})

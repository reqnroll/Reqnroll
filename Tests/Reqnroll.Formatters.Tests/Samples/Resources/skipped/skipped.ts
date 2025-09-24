import { Before, Given } from '@cucumber/fake-cucumber'

Before({tags: '@skip'}, function () {
  return 'skipped'
})

Given('a step that does not skip', function () {
  // no-op
})

Given('a step that is skipped', function () {
  // no-op
})

Given('I skip a step', function () {
  return 'skipped'
})

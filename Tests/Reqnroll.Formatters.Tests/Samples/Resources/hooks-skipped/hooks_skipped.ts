import { After, Before, Given } from '@cucumber/fake-cucumber'

Before({}, function () {
  // no-op
})

Before({ tags: '@skip-before' }, function () {
  return 'skipped'
})

Before({}, function () {
  // no-op
})

Given('a normal step', function () {
  // no-op
})

Given('a step that skips', function () {
  return 'skipped'
})

After({}, function () {
  // no-op
})

After({ tags: '@skip-after' }, function () {
  return 'skipped'
})

After({}, function () {
  // no-op
})

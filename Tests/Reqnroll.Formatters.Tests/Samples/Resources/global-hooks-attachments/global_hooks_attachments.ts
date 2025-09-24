import { When, BeforeAll, AfterAll } from '@cucumber/fake-cucumber'

BeforeAll({}, async function () {
  await this.attach('Attachment from BeforeAll hook', 'text/plain')
})

When('a step passes', function () {
  // no-op
})

AfterAll({}, async function () {
  await this.attach('Attachment from AfterAll hook', 'text/plain')
})

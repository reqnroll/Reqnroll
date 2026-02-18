import { When } from '@cucumber/fake-cucumber'
import fs from 'node:fs'

When('the string {string} is attached as {string}', async function (text: string, mediaType: string) {
  await this.attach(text, mediaType)
})

When('the string {string} is logged', async function (text: string) {
  await this.log(text)
})

When('text with ANSI escapes is logged', async function () {
  await this.log(
    'This displays a \x1b[31mr\x1b[0m\x1b[91ma\x1b[0m\x1b[33mi\x1b[0m\x1b[32mn\x1b[0m\x1b[34mb\x1b[0m\x1b[95mo\x1b[0m\x1b[35mw\x1b[0m'
  )
})

When('the following string is attached as {string}:', async function (mediaType: string, text: string) {
  await this.attach(text, mediaType)
})

When(
  'an array with {int} bytes is attached as {string}',
    async function (size: number, mediaType: string) {
    const data = [...Array(size).keys()]
    const buffer = Buffer.from(data)
    await this.attach(buffer, mediaType)
  }
)

When('a PDF document is attached and renamed', async function () {
  await this.attach(fs.createReadStream(__dirname + '/document.pdf'), {
      mediaType: 'application/pdf',
      fileName: 'renamed.pdf'
  })
})

When('a link to {string} is attached', async function (uri: string) {
    await this.link(uri)
})

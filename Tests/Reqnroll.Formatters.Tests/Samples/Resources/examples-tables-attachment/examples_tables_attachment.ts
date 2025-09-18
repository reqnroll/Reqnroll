import { When } from '@cucumber/fake-cucumber'
import fs from 'node:fs'

When('a JPEG image is attached', async function () {
  await this.attach(fs.createReadStream(__dirname + '/cucumber.jpeg'), 'image/jpeg')
})

When('a PNG image is attached', async function () {
  await this.attach(fs.createReadStream(__dirname + '/cucumber.png'), 'image/png')
})

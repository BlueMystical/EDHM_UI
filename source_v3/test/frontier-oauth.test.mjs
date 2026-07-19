import test from 'node:test';
import assert from 'node:assert/strict';
import {
  filterFrontierAuthArguments,
  findFrontierAuthCallback,
  FRONTIER_REDIRECT_URI,
  parseFrontierAuthCallback,
} from '../src/Helpers/FrontierOAuth.mjs';

test('Frontier OAuth recognizes only the registered EDHM callback', () => {
  assert.equal(FRONTIER_REDIRECT_URI, 'edhm://frontier-auth');
  assert.deepEqual(parseFrontierAuthCallback(
    'edhm://frontier-auth?code=authorization-code&state=expected-state'
  ), {
    url: 'edhm://frontier-auth?code=authorization-code&state=expected-state',
    code: 'authorization-code',
    state: 'expected-state',
    error: '',
    errorDescription: '',
  });

  assert.equal(parseFrontierAuthCallback('edhm://other-host?code=nope'), null);
  assert.equal(parseFrontierAuthCallback('edhm://frontier-auth/unregistered?code=nope'), null);
  assert.equal(parseFrontierAuthCallback('edhm://user@frontier-auth?code=nope'), null);
  assert.equal(parseFrontierAuthCallback('https://localhost:3000/edhm-callback?code=nope'), null);
});

test('Frontier OAuth extracts a callback from operating-system arguments', () => {
  const callback = 'edhm://frontier-auth?error=access_denied&error_description=Cancelled&state=abc';
  assert.equal(findFrontierAuthCallback(['--hide', callback]), callback);
  assert.equal(findFrontierAuthCallback(['--hide', `"${callback}"`]), callback);
  assert.equal(findFrontierAuthCallback(['--hide', 'not-a-url']), null);
  assert.deepEqual(
    filterFrontierAuthArguments(['--hide', callback, `"${callback}"`, '--another']),
    ['--hide', '--another']
  );
});

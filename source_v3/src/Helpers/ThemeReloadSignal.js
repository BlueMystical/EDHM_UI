import fs from 'node:fs';

// The shipped DLL polls the signal file every two seconds and can absorb the
// first change as its baseline. Send one later signal without rewriting INIs.
export function createThemeReloadSignal() {
  let pending = null;

  function cancel() {
    if (pending) clearTimeout(pending.timer);
    pending = null;
  }

  function schedule(filePath, isStillActive) {
    cancel();
    try {
      const written = fs.statSync(filePath);
      const request = {};
      pending = request;
      request.timer = setTimeout(() => {
        if (pending !== request) return;
        pending = null;
        let fd;
        try {
          if (!isStillActive()) return;
          // Open the existing file only. Preserve its contents and do not touch
          // a replacement or newer write from another application.
          fd = fs.openSync(filePath, 'r+');
          const current = fs.fstatSync(fd);
          if (current.ino !== written.ino || current.dev !== written.dev ||
              current.size !== written.size || current.mtimeMs !== written.mtimeMs) return;
          const nextWrite = new Date(Math.max(Date.now(), current.mtimeMs + 1));
          fs.futimesSync(fd, current.atime, nextWrite);
        } catch (error) {
          if (error.code !== 'ENOENT') console.warn('Theme reload retry failed:', error);
        } finally {
          if (fd !== undefined) fs.closeSync(fd);
        }
      }, 3000);
      request.timer.unref?.();
    } catch (error) {
      pending = null;
      console.warn('Could not schedule theme reload retry:', error);
    }
  }

  return { schedule, cancel };
}

export const themeReloadSignal = createThemeReloadSignal();

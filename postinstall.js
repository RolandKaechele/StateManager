// postinstall.js — StateManager
// Creates required runtime folders and optionally copies example files.

const fs   = require('fs');
const path = require('path');
const readline = require('readline');

const assetsDir  = path.resolve(__dirname, '../');
const examplesDir = path.resolve(__dirname, 'Examples');

// No additional folders required at runtime; StreamingAssets states.json is optional.
console.log('StateManager postinstall: nothing to create.');

// Copy example files with overwrite prompt
function copyFileWithPrompt(src, dest, rl, cb) {
  if (fs.existsSync(dest)) {
    rl.question(`File ${dest} exists. Overwrite? (y/N): `, answer => {
      if (answer.trim().toLowerCase() === 'y') {
        fs.copyFileSync(src, dest);
        console.log(`Overwritten: ${dest}`);
      } else {
        console.log(`Skipped: ${dest}`);
      }
      cb();
    });
  } else {
    fs.copyFileSync(src, dest);
    console.log(`Copied: ${dest}`);
    cb();
  }
}

function walkDir(dir, relBase = '') {
  let results = [];
  fs.readdirSync(dir, { withFileTypes: true }).forEach(entry => {
    const relPath = path.join(relBase, entry.name);
    const absPath = path.join(dir, entry.name);
    if (entry.isDirectory()) results = results.concat(walkDir(absPath, relPath));
    else results.push({ relPath, absPath });
  });
  return results;
}

function copyTemplates() {
  if (!fs.existsSync(examplesDir)) {
    console.log('No Examples directory found. Skipping template copy.');
    return;
  }

  const files = walkDir(examplesDir);
  if (files.length === 0) {
    console.log('No example files found.');
    return;
  }

  const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
  let i = 0;

  function next() {
    if (i >= files.length) { rl.close(); return; }
    const { relPath, absPath } = files[i++];
    const dest = path.join(assetsDir, relPath);
    const destDir = path.dirname(dest);
    if (!fs.existsSync(destDir)) fs.mkdirSync(destDir, { recursive: true });
    copyFileWithPrompt(absPath, dest, rl, next);
  }
  next();
}

copyTemplates();

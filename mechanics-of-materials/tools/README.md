# Rebuild and check the formula book

The book is generated from the formula reference, chapter examples and symbol descriptions. Edit those sources, then rebuild both outputs together.

Requirements: Node.js, Python 3 and Google Chrome. Install the pinned dependencies from this folder with npm install, then run npm run build. Set CHROME_PATH to the Chrome executable if it is not installed at the default Windows location. The build writes the offline HTML, PDF and validation.json in the formula-book folder.

Run python check_mechanics.py for numerical regression checks. From the repository root, run python tools/check_links.py for local Markdown links and git diff --check for whitespace. Render and inspect every PDF page after changing content or styling; automated checks alone do not establish that the pages are readable.

The builder checks all chapter mathematics for rendering errors. Numerical checks independently evaluate selected published examples and boundary identities; they do not automatically prove every equation in the Markdown.

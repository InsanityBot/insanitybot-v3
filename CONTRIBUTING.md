# InsanityBot Contribution Guidelines

The InsanityBot project and its associated spaces is governed by the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). If you contribute, 
you are expected to uphold this code of conduct.

## Issues

You found a problem with InsanityBot? We want to know about it! Issues can be roughly divided into two groups: bug reports and feature requests.

### Bug Reports

Use this one when something isn't working as expected. Please include as much of the following information as you can:

- The exact version and commit hash of InsanityBot
- The host system you are using (operating system, CPU architecture, any applicable control system)
- As much of the logfile you can possibly obtain. Attach this either as a file or using a service like [gists](https://gist.github.com) or a [paste](https://pastes.dev)
- A clear and concise description of what you did, what you expected to happen and what actually happened
- Any additional information you might have.

### Feature Requests

Use this one when you feel like the project could benefit from adding a new feature. Please describe it clearly and concisely, if applicable, explain the
current behavior and why it is insufficient, and why your idea would benefit the project.

---

## Pull Requests

It's awesome to know you wish to contribute to the InsanityBot project! Before you do so though, please give these guidelines a quick read.

### Contributor License Agreement (CLA)

By submitting changes to this repository, you are hereby agreeing that:

- Your contributions will be licensed irrecoverably under the MIT license.
- Your contributions are of your own work and free of legal restrictions (such as patents and copyrights) or other issues which would pose issues for inclusion or distribution under the above license.

If you do not agree to these terms, please do not submit contributions to this repository. 
If you have any questions about these terms, feel free to get in contact with the InsanityBot Team through the Discord server or through opening an issue.

### Code Style

When contributing code changes to the project, make sure to run `dotnet format` before submitting a PR. This will ensure your changes follow the rules
defined in the .editorconfig file. Additionally, respect the following guidelines:

- Avoid lines longer than a normal screen.
- Avoid immensely complicated statements.
- Document complicated or low-level logic.
- Localize all user-facing strings.
- Avoid hard-coding constants that are not self-evident.
- Use the logger.

### Optimization

Optimizing is fun, optimizing is great! However, if you make a pull request for performance' sake, we'd like to see some benchmarks and maybe 
IL and Assembly comparisons.

Some parts also just don't need to be optimized at all. Background logic running every twenty seconds does not need brutal optimization. Focus your efforts
elsewhere.

### Making a Pull Request

Your PR should include a brief description of the changes it makes and link to any open issues it resolves. Also, once again, make sure to follow the
code style guidelines.

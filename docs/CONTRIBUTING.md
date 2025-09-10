# Contributing to YASEM

Welcome to YASEM! We appreciate your interest in contributing to our open-source project. By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Your contributions are valuable, and together we can create an even better project.

## Code of Conduct

We are committed to fostering an inclusive and welcoming environment for all contributors. Please review and abide by the [Code of Conduct](CODE_OF_CONDUCT.md). Remember to treat others with respect and kindness throughout your interactions with the project.

## How Can I Contribute?

### General Guidelines

*   **Be respectful and constructive**: Engage in discussions and code reviews with a positive and helpful attitude.
*   **Follow existing conventions**: When contributing code, try to match the existing style, naming conventions, and architectural patterns of the project.
*   **Test your changes**: Ensure your contributions are well-tested. If you're adding new features, please include appropriate unit or integration tests.
*   **Keep it focused**: Each pull request should address a single issue or feature. Avoid mixing unrelated changes.

### Code Style

*   **C# Naming Conventions**: Follow standard C# naming conventions (e.g., PascalCase for classes and methods, camelCase for local variables).
*   **Formatting**: Use the default .NET formatting settings. You can run `dotnet format` to automatically format your code.
*   **Comments**: Add comments where necessary to explain complex logic or design decisions. Avoid redundant comments that merely restate the obvious.

### Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification. This helps us create an explicit development history, which makes it easier to write automated changelogs and understand the purpose of each commit.

Each commit message should be structured as follows:

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

#### Type

Must be one of the following:

*   **feat**: A new feature
*   **fix**: A bug fix
*   **docs**: Documentation only changes
*   **style**: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc.)
*   **refactor**: A code change that neither fixes a bug nor adds a feature
*   **perf**: A code change that improves performance
*   **test**: Adding missing tests or correcting existing tests
*   **build**: Changes that affect the build system or external dependencies (example scopes: gulp, broccoli, npm)
*   **ci**: Changes to our CI configuration files and scripts (example scopes: Travis, Circle, BrowserStack, SauceLabs)
*   **chore**: Other changes that don't modify src or test files
*   **revert**: Reverts a previous commit

#### Scope

The scope should indicate the part of the codebase affected by the change. Examples: `cli`, `core`, `connectors`, `validators`, `config`, `docs`, etc.

#### Description

A concise, imperative description of the change.

*   Use the imperative mood ("add", "change", "fix")
*   Do not capitalize the first letter
*   Do not end with a period

#### Body (Optional)

Provide more contextual information about the change.

#### Footer (Optional)

Reference issues or pull requests. For example: `Closes #123`, `Refs #456`.

**Example Commit Message:**

```
feat(core): Add support for IMAP and POP3 mail server types

This commit introduces the ability to configure and connect to both IMAP and POP3 mail servers.
The MailOptions model has been updated to include a MailServerType property,
and the EmailConnector now dynamically adapts its connection logic based on this setting.
```

## Pull Request Process

To contribute to YASEM, follow these steps:

1.  Fork the repository to your own GitHub account.
2.  Create a branch with a descriptive name for your feature or bug fix.
3.  Make your changes and ensure the code is well-documented and tested.
4.  Commit your changes with clear and concise commit messages following the guidelines above.
5.  Push your changes to your fork.
6.  Create a pull request (PR) to the main repository's `main` branch.

Please make sure your pull request:

*   Includes a clear description of the changes and the problem they solve (and the issue number where applicable).
*   Is well-documented and follows the project's coding style.
*   Passes all tests and doesn't introduce new issues.

## Pull Request Acceptance

We welcome your pull requests, and we appreciate the time you invest in contributing to YASEM. However, it's important to note that acceptance of pull requests is at the discretion of the project maintainers. While we value every contribution, we reserve the right to make the final decision regarding merging or rejecting pull requests based on factors such as alignment with project goals, code quality, technical feasibility and even time availability.

We encourage constructive discussions and collaboration around pull requests. Feel free to engage in conversations about your proposed changes, as this can lead to improvements and enhancements.

## Reporting Bugs

If you find a bug, please open an issue on our [GitHub Issues page](https://github.com/your-username/YASEM/issues). When reporting a bug, please include:

*   A clear and concise description of the bug.
*   Steps to reproduce the behavior.
*   Expected behavior.
*   Actual behavior.
*   Screenshots or error messages if applicable.
*   Your operating system and YASEM version.

## Suggesting Enhancements

We love new ideas! If you have a suggestion for an enhancement, please open an issue on our [GitHub Issues page](https://github.com/your-username/YASEM/issues). Please include:

*   A clear and concise description of the proposed enhancement.
*   Why you think this enhancement would be valuable.
*   Any potential alternatives or considerations.

Thank you for your understanding and your interest in making YASEM better! If you have any questions or need clarification about the contribution process, please don't hesitate to reach out to us. We appreciate your dedication to the project and look forward to your contributions.

*Happy Contributing!*

---
*This document is subject to change as the project evolves. Please check back for updates.*

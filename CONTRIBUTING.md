# Contributing to YASEM

We welcome contributions to YASEM! By participating in this project, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md).

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

### Reporting Bugs

If you find a bug, please open an issue on our [GitHub Issues page](https://github.com/your-username/YASEM/issues). When reporting a bug, please include:

*   A clear and concise description of the bug.
*   Steps to reproduce the behavior.
*   Expected behavior.
*   Actual behavior.
*   Screenshots or error messages if applicable.
*   Your operating system and YASEM version.

### Suggesting Enhancements

We love new ideas! If you have a suggestion for an enhancement, please open an issue on our [GitHub Issues page](https://github.com/your-username/YASEM/issues). Please include:

*   A clear and concise description of the proposed enhancement.
*   Why you think this enhancement would be valuable.
*   Any potential alternatives or considerations.

## Basics & Requirements

![Korpi banner image](img/banner.png)

### What is Korpi?

Korpi is a voxel[*](#voxel-engine-vs-korpi) engine written in C#, that uses OpenGL as the rendering backend. I'm working on the engine to make it more stable and feature complete.

The long-term goal of this project is to eventually develop into its own game, with player-hosted servers and official modding support.

### What is OpenGL?

[OpenGL](https://en.wikipedia.org/wiki/OpenGL) is a cross-platform graphics-rendering library, originally developed by [Silicon Graphics (SGI)](https://www.sgi.com), and now maintained by the [Khronos group](https://www.khronos.org).  OpenGL is used for everything from video games to CAD tools to web browsers to mobile phones.

### What operating systems does Korpi run on?

Korpi runs on .NET 6.0, and **has currently been only tested on Windows**. However, it should be possible to run it on Linux and macOS as well, as long as the required dependencies are met.

If you'd like to help us test Korpi on other operating systems, please let us know!

## Contributing

I (Japsu) appreciate your interest in contributing to the project!
All contributions are welcome, and I'm happy to help anyone who wants to contribute!

Whether you're a developer, designer, or enthusiast, there are various ways you can contribute and help improve Korpi:
- [Contributing to source](#how-can-i-contribute-to-the-source)
- [Contributing to documentation](#how-can-i-contribute-to-the-docs)
- [Reporting issues](#how-can-i-report-issues)
- [Testing](#how-can-i-help-test-korpi)

There is also a more detailed [Contributing Guide](https://github.com/japsuu/Korpi/blob/master/CONTRIBUTING.md) available on GitHub.

### How can I contribute to the source?

1. **Fork the Repository:**
    - Fork the [repository](https://github.com/japsuu/Korpi) to your GitHub account.

2. **Clone the Repository:**
    - Clone the forked repository to your local machine.
      ```bash
      git clone https://github.com/your-username/Korpi.git
      ```

3. **Create a Branch:**
    - Create a new branch for your contribution. Name the branch according to the type of contribution you are making.
    - [Naming guide](https://dev.to/varbsan/a-simplified-convention-for-naming-branches-and-commits-in-git-il4)
      ```bash
      git checkout -b feature-your-feature
      ```

4. **Make Changes:**
    - Implement the changes or new features in your branch.
    - Ensure your code follows the project's coding standards.

5. **Test Your Changes:**
    - Test your changes thoroughly to make sure they work as expected.
    - If applicable, update or create tests to cover your code.

6. **Commit Your Changes:**
    - Commit your changes with a clear and descriptive commit message.
      ```bash
      git commit -m "Add feature: your feature description"
      ```

7. **Push Changes:**
    - Push your changes to your forked repository.
      ```bash
      git push origin feature-your-feature
      ```

8. **Create a Pull Request:**
    - Open a pull request (PR) from your branch to the main repository's `master` branch.
    - Provide a detailed description of your changes in the pull request.
    - Reference any relevant issues in the description.

9. **Code Review:**
    - Participate in the code review process, address any feedback, and make necessary changes.

10. **Merge:**
- Once your changes are approved, they will be merged into the main repository.

### How can I contribute to the docs?

First of all, you should read the [Contributing to source](#how-can-i-contribute-to-the-source) section above, as the process is very similar.

Second, please contact me (Japsu) on the [Korpi Discord](https://discord.gg/AhSX58wmWG) or [open an issue](https://github.com/japsuu/Korpi/issues/new/choose) before starting work on any documentation changes, to avoid duplicate work.

Once your issue has been approved, you can start working on your changes. When you're done, open a pull request (PR) from your branch to the main repository's `master` branch.

### How can I report issues?

If you encounter any issues or have suggestions for improvements, please [open an issue](https://github.com/japsuu/Korpi/issues/new/choose) on the GitHub repository.

### How can I help test Korpi?

If you'd like to help test Korpi, please contact me (Japsu) on the [Korpi Discord](https://discord.gg/AhSX58wmWG) or [open an issue](https://github.com/japsuu/Korpi/issues/new/choose)

## Miscellaneous

### Voxel engine vs korpi
Truth be told, this engine is not a "true" voxel engine.
This engine uses [voxel data representation](https://en.wikipedia.org/wiki/Voxel) but does not use voxel volume rendering techniques ([like volume marching](https://en.wikipedia.org/wiki/Volume_ray_casting)), but instead opts for traditional polygonal rendering.
When referring to voxels, I mean blocks ;)
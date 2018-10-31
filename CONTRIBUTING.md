# Contributing to the Fish Orientation Tracking System

Hello there! Thanks for coming by. You'll always be welcome regardless no matter what.

This document is a set of guidelines for contributing to the project. 
These are just guidelines, but they are here to ensure an efficient and foolproof workflow. 
Use your best judgment if in doubt, or ask on the [Discord Server](https://discord.gg/Z3sPhyS)

This whole document is derived from the [Atom CONTRIBUTING.md](https://github.com/atom/atom/blob/master/CONTRIBUTING.md). If something looks like they wrote, then they probably did.

### Table Of Contents

[Code of Conduct](#code-of-conduct)

[I Just Have A Question](#i-just-have-a-question)

[I'm New. What Do I Need To Know?](#im-new-what-do-i-need-to-know)
  * [A Brief History Of Space Station 13](#a-brief-history-of-space-station-13)
  * [Game Design Document](#game-design-document)
  * [Code Maintainers](#code-maintainers)

[How To Contribute](#how-to-contribute)
  * [Reporting Bugs](#reporting-bugs)
  * [Suggesting Enhancements](#suggesting-enhancements)
  * [Code or Asset Contributions](#code-or-asset-contributions)
  * [Pull Requests](#pull-requests)

[Styleguides](#style-guides)
  * [Art Style](#art-style)
  * [C# Styleguide](#c-styleguide)
  * [Git Commit Messages](#git-commit-messages)

## Code of Conduct

This project and everyone participating in it are governed by the [Code of Conduct](CODE_OF_CONDUCT.md). 
By participating, you are expected to uphold this code. 
Please report any unacceptable behavior to an admin.

## I Just Have A Question

> **Note:** Please don't file an issue to ask a question. You'll get faster results by asking on the Discord.

There is a discord server with plenty helpful people [right over here](https://discord.gg/Z3sPhyS)

## I'm New. What Do I Need To Know?

### A Brief History Of Space Station 13

> *From the [SS13 website](https://spacestation13.com/)*

Space Station 13 is a community developed, multiplayer round-based role playing game, where players assume the role of a crewmember on a space station. 
Together they must keep the station running smoothly, whilst dealing with antagonistic forces who threaten to sabotage the mission.

At the beginning of each round, players select a crew member role on the station. 
These range from high up positions like the captain and heads of staff, to engineers, scientists, medical doctors, security officers, all the way down to the lower responsibility roles such as the janitor and lowly assistant. 
At round start, one or more players will be given an antagonistic role at random, and a secret objective that’s very likely to cause disruption to the mission at hand.

When the crew aren’t turning on each other through sheer paranoia, they will face various dangers depending on the round: Sleeper agents hell bent on sabotage, shape-shifting aliens, RPG toting syndicate operatives and more. 
Not to mention the occupational hazards of working in space, such as decompression, meteor showers, radiation storms, airlock mishaps, rogue AI and catastrophic engine failure.

### Code Maintainers

A set of people are responsible for making sure the runs smoothly and that the contributions made to project are wanted, correct, and functional.
These people are refered to as maintainers.

The project maintainers have the follow goals:
* To prioritize issues and pull requests so the most urgent and most wanted changes gets into the build quickly.
* To make sure even minor contributions don't get overlooked. All contributions are created equal.
* Make sure the contributions comply with our standards, will not break the project, and are wanted, before they are merged.

## How To Contribute

There many things that you can do to help out, regardless if you're a maintainer or a visitor, a coder or a player. At the most basic, reporting bugs is essential to making everything run flawlessly. People with some skills and time to spare can help out the community by contributing some work. Any help is appreciated.

### Reporting Bugs

Reporting bugs might be one of the easiest and most essential things you can do. This section outlines what a proper bug report looks like.
The maintainers (and other people like you) need to understand your report, be able to reproduce the behavior and find related reports easily.
If your bug report meets all three of those conditions, then we can quickly and efficiently be handled, and you have directly improved the experience for others.

Before creating a bug report, please check [this list](#before-submitting-a-bug-report) as you might find out that you don't need to create one. 
When you are creating a bug report, please [include as many relevant details as possible](#how-do-i-submit-a-good-bug-report). Feel free to remove any non-applicable junk.
Use a template when creating the issue, as the information it asks for helps you help us help you more efficiently.

#### Before Submitting A Bug Report

* **Do a quick search in existing issue** to see if the problem has already been reported. 
* If it has **and the issue is still open**, add a comment to the existing issue instead of opening a new one.
* If you find a **Closed** issue that seems like it is the same thing that you're experiencing, open a new issue and include a **link** to the original issue in the body of your new one.

#### How Do I Submit A (Good) Bug Report?

Bugs are tracked as [GitHub issues](https://guides.github.com/features/issues/). 
Your first step in creating the issue is to use a template when creating your bug report.

Explain the problem clearly and include additional details to help maintainers reproduce the problem:

* **Use a clear and descriptive title** for the issue to identify the problem.
* **Describe the steps which reproduce the problem** in as many details as possible. Be specific as any detail might matter.
* **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that behavior.
* **Explain which behavior you expected to see instead and why.**
* **Include screenshots and/or animated GIFs** which show you following the steps and demonstrate the problem.
* **If you're reporting that the client crashed**, include a crash report with the error log. It is located depending on your operating system, so follow [this official guide](https://docs.unity3d.com/Manual/LogFiles.html). 
* **If the problem wasn't triggered by a specific action**, describe what you were doing before the problem happened.
* **Include which version of the client are you running?** The version number should be written on the first screen when you launch the game.

### Suggesting Enhancements

You may have a good idea on how to improve the game, whether it be a new feature or a minor improvement. We want those ideas.
Submitting suggestions is functionally similar to reporting bugs but without all the boring stuff.

When you are creating an enhancement suggestion, please [include as many details as possible](#how-do-i-submit-a-good-enhancement-suggestion). 
Use the templates as usual, including the steps that you imagine you would take if the feature you're requesting existed.

> *Note:* This is only for suggestions. If you have produced actual code or assets, go the [next section](#code-or-asset-contributions)

#### How Do I Submit A (Good) Suggestion?

Enhancement suggestions are tracked as [GitHub issues](https://guides.github.com/features/issues/). If you have an idea for an enhancement, you should provide the following information in the issue:

* **Use a clear and descriptive title** for the issue to identify the suggestion.
* **Provide a step-by-step description of the suggested enhancement** in as many details as possible.
* **Describe the current behavior** and **explain which behavior you expected to see instead** and why.
* **Explain why this enhancement would be useful** to most Atom users and isn't something that can or should be implemented as a [community package](#atom-and-packages).

### Code or Asset Contributions

Making an actual change in the project is really admirable and also a simple but complicated process. This section will guide you through said process.

Regardless of kind of asset you produce remember to head over to the [Style guide section](#style-guides) to make sure your contribution fits in with everyone else's.

After reading through the next subsections don't forget to head over to the [Pull Requests section](#pull-requests) on how to submit your changes.

#### Tools and Setup

The game is developed in **Unity 2018.2**. Regardless of if you make code or art, installing Unity will be helpful for getting your contribution in. Upgrading to the next patch version of Unity is perfectly acceptable, but avoid downgrading. The plan is to upgrade to the **2018.4 LTS** version when it comes out and stay on that version until a new feature warrants an upgrade, or support stops.

For coding, you can use whatever you want, but [Visual Studio](https://visualstudio.microsoft.com/) is probably the most popular choice. It's free and integrates well with Unity. If you're a student, we recommend getting a copy of [ReSharper](https://www.jetbrains.com/resharper/?gclid=CjwKCAjwq57cBRBYEiwAdpx0vQ8IFQoIbF0cUP89TCZbITKIMsDYGVqQIJXBMfXodtZzL35s5g0BiBoC2qAQAvD_BwE&gclsrc=aw.ds.ds&dclid=CNuQ2IKdlN0CFY3QdwoducANHg), which adds a lot of code hints and shortcuts that will significantly improve your code quality.

The art style guide recommends using **Blender** for 3D models, but as long as the look and performance match the existing assets, any 3D modeling tool is allowed.

To get a hold of the project, you need **git client**. Git is the software that manages the source. GitHub is the website that we use to host it. Some of the popular options are:

* [GitHub Desktop](https://desktop.github.com/), recommended for beginners
* [Sourcetree](https://www.sourcetreeapp.com/)
* [Smart Git](https://www.syntevo.com/smartgit/)
* [Git Kraken](https://www.gitkraken.com/)
* [Git CLI](https://git-scm.com/), if you know what you're doing

Regardless of what client you use, we recommend installing the latest version of git separately, to make sure you have all the latest features.

#### Forking the repository

Once you have your tools in order, you should fork this repository on GitHub, using the Fork button on this page. This naturally requires a GitHub account. You will commit your changes to that repository and make a [pull request](#pull-requests) to merge it into our repository.

After forking, you should clone your repository to your local machine. This might take a whíle. Once it's done cloning, remember to swich to the develop branch, and maybe even delete the master branch. When that is done, you're ready to start.

> **Note:** Do *not* clone this repository, as you will not have the rights to push to it. Only clone your own fork.

Soon you will probably find your fork to be out of date. GitHub has made a [pretty clear guide](https://help.github.com/articles/syncing-a-fork/) on how to sync your fork so it is up to date with the shared repository.

#### Git Flow

The repository is structured based on a subset of [GitFlow](https://nvie.com/posts/a-successful-git-branching-model/), using only using three types of branches: the *master* branch, the *develop* branch, and *feature* branches.

The *master* branch always contains the latest released version. No one is allowed to push directly to the *master* branch, not even maintainers. The only exception is if structural changes need to be applied to the repository.

The *develop* branch will contain the latest accepted code. This is the branch that pull requests should merge into. Every once in a while the *develop* branch will be merged into *master*, and a new build will be released with the new changes.

The *feature* branches will contain your unmerged changes. These branches lives on your fork of the repository and will be the source of your pull request. Note that there should ideally only be one feature per branch. Huge merge requests take long to merge, and parts of it might be able to be merged sooner if it is on it's own branch.

### Pull Requests

Pull requests allow the maintainers to verify that any changes are wanted and don't break the existing code before they are merged into the latest version. Note that pull requests should merge from your own *develop* or *feature* branch into this repository's *develop* branch.

Pull requests are mandatory for any changes and are required to be reviewed by maintainers and should pass any CI test.

Here are some general guidelines for pull requests:
* Fill in [the template](.github/PULL_REQUEST_TEMPLATE.md). It'll pop up when you create the Pull Request.
* Do not include issue numbers in the PR title
* Include screenshots and/or animated GIFs in your pull request whenever possible.

## Style guides

Follow these guidelines for good luck.

### Art Style

The art style of all assets should adhere to the **[Art Style Guide](StyleGuides/ART.md)**. The style guide was created by *beep.

### C# Styleguide

All C# code should follow the **[C# Style Guide](StyleGuides/C_SHARP.md)**. The style is open to debate, especially when it is about clarity or readability, but anything that comes down purely to personal preference should fall back on official style guides or community convention.

### Git Commit Messages

* These are just good habits. Pull requests take priority if you want to save your energy.
* Use the present tense ("Add feature" not "Added feature")
* Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally after the first line
* Consider starting the commit message with an applicable emoji:
    * :art: `:art:` when improving the format/structure of the code
    * :racehorse: `:racehorse:` when improving performance
    * :penguin: `:penguin:` when fixing something on Linux
    * :apple: `:apple:` when fixing something on macOS
    * :checkered_flag: `:checkered_flag:` when fixing something on Windows
    * :bug: `:bug:` when fixing a bug
    * :fire: `:fire:` when removing code or files
    * :green_heart: `:green_heart:` when fixing the CI build
    * :white_check_mark: `:white_check_mark:` when adding tests
    * :lock: `:lock:` when dealing with security

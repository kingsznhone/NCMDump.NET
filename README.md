# NCMDump.NET

Decipher .ncm file to mp3 || flac

## Feature

Highly Optimized Corelib.

Keep all ID3 tags and cover image. Very nice.

## Changelog

### v1.3

Bug fix.

Async optimization.

### v1.2

Bug fix.

Drag directory || Select folder on GUI support.

### v1.1

Drag directory on executable support.

Bug fix

## Usage

### CLI

Drag .ncm file or directory on exe

### GUI

Don't ask, Just use.

## System Requirement

### OS : Windows 10 10.0.19041.0 with .NET6 Runtime

## API

### public bool Convert(string path)

Convert NCM file into MP3/FLAC format in synchronous.

#### Parameters

```path``` String

File Path to a NCM file.

#### Return

```bool``` Is convert success

<br/>

### public async Task\<bool\> ConvertAsync(string path)

Convert NCM file into MP3/FLAC format in asynchronous.

#### Parameters

```path``` String

File Path to a NCM file.

#### Return

```bool``` Is convert success

## Refrence

<https://github.com/anonymous5l/ncmdump>

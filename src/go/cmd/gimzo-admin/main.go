package main

import (
	"fmt"
	"os"
	"strings"
	"time"
)

type Config struct {
	Name     string
	Verbose  bool
	ShowHelp bool
	Import   bool
}

func main() {
	start := time.Now()

	exitCode := 0

	cfg, err := parseArgs(os.Args[1:])

	if err != nil {
		showHelp(err)
		exitCode = 1
		goto finish
	}

	if cfg.ShowHelp {
		showHelp(nil)
		goto finish
	}

	fmt.Println("Verbose  = ", cfg.Verbose)
	fmt.Println("ShowHelp = ", cfg.ShowHelp)
	fmt.Println("Import   = ", cfg.Import)

finish:
	fmt.Printf("\n%s completed in %v\n", cfg.Name, time.Since(start))
	fmt.Printf("\nExiting with code: %d\n", exitCode)
	os.Exit(exitCode)
}

func parseArgs(args []string) (Config, error) {
	cfg := Config{
		Name:     "gimzo-admin",
		Verbose:  false,
		ShowHelp: false,
		Import:   false,
	}

	for a := 0; a < len(args); a++ {
		argument := strings.ToLower((args[a]))

		switch argument {
		case "-v", "--verbose":
			cfg.Verbose = true
		case "-h", "--help", "help", "?", "-?":
			cfg.ShowHelp = true
		case "--import":
			cfg.Import = true
		default:
			return cfg, fmt.Errorf("Unknown argument: %s", args[a])
		}
	}

	return cfg, nil
}

func showHelp(err error) {

	if err != nil {
		fmt.Printf("\n%s\n", err.Error())
	}
	fmt.Printf("\nUsage\n")
	fmt.Printf("%s\n", strings.Repeat("-", 80))
	fmt.Printf("%s\t%s\n", "-h | -? | ? | help | --help", "Show this help.")
	fmt.Printf("%-27s\t%s\n", "-v | --verbose", "Turn on verbose messaging.")
	fmt.Printf("%-27s\t%s\n", "--import", "Perform import from financialdata.net")
}

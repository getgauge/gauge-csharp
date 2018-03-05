// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-Java.

// This program is free software.
//
// It is dual-licensed under:
// 1) the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version;
// or
// 2) the Eclipse Public License v1.0.
//
// You can redistribute it and/or modify it under the terms of either license.
// We would then provide copied of each license in a separate .txt file with the name of the license as the title of the file.


using System;
using System.Collections.Generic;
/**
* Gives the information about the current execution at runtime - spec, scenario, step that is running.
*/

namespace Gauge.CSharp.Lib {

    public class ExecutionContext {
        private Specification currentSpecification;
        private Scenario currentScenario;
        private StepDetails currentStep;

        public ExecutionContext(Specification specification, Scenario scenario, StepDetails stepDetails) {
            this.currentSpecification = specification;
            this.currentScenario = scenario;
            this.currentStep = stepDetails;
        }

        public ExecutionContext() {
            this.currentSpecification = new Specification();
            this.currentScenario = new Scenario();
            this.currentStep = new StepDetails();
        }

        /**
        * @return - The Current Specification that is executing.
        * Returns null in BeforeSuite and AfterSuite levels as no spec is executing then.
        */
        public Specification getCurrentSpecification() {
            return currentSpecification;
        }

        /**
        * @return - The Current Scenario that is executing.
        * Returns null in BeforeSuite, AfterSuite, BeforeSpec levels as no scenario is executing then.
        */
        public Scenario getCurrentScenario() {
            return currentScenario;
        }

        /**
        * @return - The Current Step that is executing.
        * Returns null in BeforeSuite, AfterSuite, BeforeSpec, AfterSpec, BeforeScenario levels as no step is executing then.
        */
        public StepDetails getCurrentStep() {
            return currentStep;
        }

        /**
        * @return - All the valid tags (including scenario and spec tags) at the execution level.
        */
        public List<String> getAllTags() {
            HashSet<String> specTags = new HashSet<String>(currentSpecification.getTags());
            foreach (var tag in currentScenario.getTags()){
                specTags.Add(tag);    
            }
            return new List<String>(specTags);
        }

        public class Specification {
            private String name = "";
            private String fileName = "";
            private Boolean isFailing = false;
            private IEnumerable<String> tags;

            public Specification(String name, String fileName, bool isFailing, IEnumerable<String> tags) {
                this.name = name;
                this.fileName = fileName;
                this.isFailing = isFailing;
                this.tags = tags;
            }

            public Specification() {
                tags = new List<String>();
            }

            /**
            * @return List of all the tags in the Spec
            */
            public IEnumerable<String> getTags() {
                return tags;
            }

            /**
            * @return True if the current spec is failing.
            */
            public Boolean getIsFailing() {
                return isFailing;
            }

            /**
            * @return Full path to the Spec
            */
            public String getFileName() {
                return fileName;
            }

            /**
            * @return The name of the Specification as mentioned in the Spec heading
            */
            public String getName() {
                return name;
            }
        }
        public class StepDetails {
            private String text = "";
            private Boolean isFailing = false;

            public StepDetails(String text, bool isFailing) {
                this.text = text;
                this.isFailing = isFailing;
            }

            public StepDetails() {
            }

            /**
            * @return True if the current spec or scenario or step is failing due to error.
            */
            public Boolean getIsFailing() {
                return isFailing;
            }

            /**
            * @return The name of the step as given in the spec file.
            */
            public String getText() {
                return text;
            }

        }

        public class Scenario {
            private String name = "";
            private Boolean isFailing = false;
            private IEnumerable<String> tags;

            public Scenario(String name, bool isFailing, IEnumerable<String> tags) {
                this.name = name;
                this.isFailing = isFailing;
                this.tags = tags;
            }

            public Scenario() {
                tags = new List<String>();
            }

            /**
            * @return List of all tags in just the scenario
            */
            public IEnumerable<String> getTags() {
                return tags;
            }

            /**
            * @return True if the scenario or spec is failing
            */
            public Boolean getIsFailing() {
                return isFailing;
            }

            /**
            * @return Name of the Scenario as mentioned in the scenario heading
            */
            public String getName() {
                return name;
            }

        }
    }
}
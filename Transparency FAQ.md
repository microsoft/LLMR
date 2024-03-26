# Large Language Model for Mixed Reality (LLMR): Responsible AI FAQ 

## What is LLMR? 

LLMR is a framework for the real-time creation and modification of interactive Mixed Reality experiences using LLMs in the Unity game engine.  

LLMR is an orchestration of various LLM modules that significantly improves the reliability of generating functioning interactive 3d objects, tools and scenes over a standard code-writing LLM. 

## What can LLMR do? 

LLMR is a general purpose tool allowing for run-time creation of dynamical content in 3d environments, by combining LLMs with a run-time compiler inside of the rendering engine. 

By incorporating techniques for scene understanding, task planning, self-debugging, and memory management, LLMR outperforms the standard GPT-4 by 4x in average error rate. 

Examples of content that can be created with LLMR include:  
- Creation of a kitchen scene from scratch using Unity primitives.  
- Prompting and drawing objects into existence via multi-modal interactions.  
- Integration with external plugins like SketchFab to create high-fidelity scenes and prompting skills like animation creation.  
- Edit existing 3d scenes.  
- Automated generation of instructional guides and QA with scene knowledge 
- Creation of tools on the fly, from accessibility for color-blind individuals to tools for drawing on any virtual surface or deforming meshes. 

LLMR can be readily extended to novel APIs via a simple Skill Library LLM module 

## What is/are LLMR’s intended use(s)? 

The main intended use for LLMR is spontaneous creation of content inside of an immersive XR or gaming 3d environment. 

Another use is in faster prototyping of interactive elements during development of 3d content. 

## How was LLMR evaluated? What metrics are used to measure performance? 

As an orchestrated pipeline, LLMR augments an LLM coder with multiple modules to enhance its reliability. To empirically justify the inclusion of each module in our framework, we quantitatively evaluated LLMR's generative performance against a variety of prompts and baselines.  

In addition to success in compiling the generated code (rate of compilation and runtime errors), we evaluated how our framework meets our design goals: real-timeness, complexity of interaction, and iterative fine-tuning ability.  

We evaluated LLMR on single prompts in an empty and existing scene, highlighting the impact of each module and overall performance compared to standard LLMs. We also evaluated the framework's performance at completing tasks with different complexity. Then, we conducted a similar experiment on sequential prompts to illustrate LLMR's capacity for iterative designs. 

Our findings underscore LLMR's superior capability in programming virtual scenes when compared to a standalone general-purpose language model. 

Additionally, to evaluate the quality of the generated output we ran a user study. 

## What are the limitations of LLMR? How can users minimize the impact of LLMR’s limitations when using the system? 

The potential harmful, false or biased responses to users prompts would likely be unchanged compared to the base LLM used in the framework.  

LLMR may struggle to implement more complex tasks, but this can be mitigated by interacting with the Planner component to break up difficult tasks into simpler ones. 

While the success rate of writing code is greatly enhanced using LLMR, this comes at the cost of multiple LLM API calls, which leads to both longer wait times as well as higher token cost. This can be limited by reducing the number of times the Inspector module is allowed to correct the code written by the Builder module. 

Biases and toxicity: LLMR inherits the various societal biases and toxicity of potential generated outputs from the base LLM used in the framework, and as such is as safe to use as any of these models hosted on Azure OpenAI. 

## What operational factors and settings allow for effective and responsible use of LLMR? 

Users can choose which C# code namespaces are available for runtime compilation. By default, we limit these to a reasonable subset that does not allow compilation of unsafe code. 

For more information on transparency, see https://learn.microsoft.com/en-us/legal/cognitive-services/openai/transparency-note?tabs=text
